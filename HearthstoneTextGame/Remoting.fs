namespace HearthstoneTextGame

open System.Collections.Generic
open IntelliFactory.WebSharper

open Game

type Response<'T> =
    | Success of 'T
    | Error of string

type GameEvent () =

    let gameChangeEvt = new Event<_>()

    [<CLIEvent>]
    member this.GameChanged = gameChangeEvt.Publish
    member this.Event = gameChangeEvt

module Remoting =
    
    // #region Game DB
    let gameDb = new Dictionary<Guid, System.DateTime * GameSession>()
    let gameEventDb = new Dictionary<Guid, GameEvent>()

    let updateGame (game : GameSession) =
        let now = System.DateTime.Now
        match gameDb.ContainsKey(game.Guid) with
        | true ->
            let time, gameInDb = gameDb.Item(game.Guid)
            if now.CompareTo(time) < 0 then
                failwith ("Update data in the past")
            if game <> gameInDb then
                gameDb.Remove(game.Guid) |> ignore
                gameDb.Add(game.Guid, (now, game))
                gameEventDb.Item(game.Guid).Event.Trigger()
        | false ->
            gameDb.Add(game.Guid, (now, game))
            gameEventDb.Add(game.Guid, GameEvent())

    let getEvent (gameGuid : Guid) =
        match gameEventDb.ContainsKey(gameGuid) with
        | false -> None
        | true -> Some <| gameEventDb.Item(gameGuid)

    let getGameTime gameGuid =
        match gameDb.ContainsKey(gameGuid) with
        | false -> None
        | true ->
            gameDb.Item(gameGuid) |> fst |> Some

    let doesGameExist gameGuid =
        match gameDb.ContainsKey(gameGuid) with
        | false -> None
        | true ->
            gameDb.Item(gameGuid) |> snd |> Some
    // #endregion

    // #region Helper method
    let respond gameGuid callback =
        match doesGameExist gameGuid with
        | Some game ->
            callback game
        | None ->
            Error("Game does not exist")

    let respondAsync gameGuid action ifSuccess ifFailure =
        async {
            return respond gameGuid
                    (fun game ->
                        match action game with
                        | Some result ->
                            ifSuccess (result)
                        | None ->
                            ifFailure
                    )
        }
    // #endregion

    // #region Synchronous helper functions
    [<Remote>]
    let GetPlayableClass () = Hero.playableClasses

    [<Remote>]
    let GetPredefinedDeck () = async { return Deck.PredefinedDecks }

    [<Remote>]
    let GetICharName (guid : Guid) (gameGuid : Guid) =
        respondAsync
            gameGuid
            (fun game ->
                let name = ref ""
                findIChar 
                    guid
                    game 
                    (fun hero -> 
                        game.Players 
                        |> List.tryFind(fun e -> e.Face.Guid = guid)
                        |> Option.iter(fun player -> name := "Face: " + player.Name)
                    )
                    (fun minion -> name := "Minion: " + minion.Card.Name) |> ignore
                if name.Value = "" then None
                else Some name.Value
            )
            (fun name -> Success(name))
            (Error("Cannot find character"))
        |> Async.RunSynchronously

    [<Remote>]
    let GetCard (cardId : string) =
        async {
            match Card.playableCards |> List.tryFind(fun e -> e.Id = cardId) with
            | Some card -> return Success(card)
            | None -> return Error("Cannot get card")
        }
        |> Async.RunSynchronously

    [<Remote>]
    let AskForUpdate (gameGuid : Guid) =
        async {
            try
                let event = (getEvent (gameGuid)).Value.GameChanged
                let waitTask = Async.RunSynchronously (Async.AwaitEvent event, 30 * 60 * 1000) |> ignore
                // TODO: check if game ended -> no more update
                return gameGuid, true
            with
            | e -> return gameGuid, false
        }
       
    [<Remote>]
    let NumberOfGame () =
        async {
            return gameDb.Count
        }
    // #endregion

    // #region Game administration functions
    [<Remote>]
    let NewGame () =
        async {
            let newGame = GameSession.Init()
            updateGame newGame
            return Success(newGame.Guid)
        }

    [<Remote>]
    let RegisterPlayer (playerName : string) (gameGuid : Guid) =
        respondAsync
            gameGuid
            (registerRandomDeckPlayer playerName)
            (fun (player, newGame) ->
                updateGame newGame
                Success (player.Guid)
            )
            (Error ("Cannot register player"))

    [<Remote>]
    let RegisterPlayerWithClass (playerName : string) (playerClass : string) (gameGuid : Guid) =
        respondAsync
            gameGuid
            (registerRandomDeckPlayerWithClass playerName playerClass)
            (fun (player, newGame) ->
                updateGame newGame
                Success (player.Guid)
            )
            (Error ("Cannot register player"))

    [<Remote>]
    let RegisterPlayerWithDeck (playerName : string) (deck : Deck) (gameGuid : Guid) =
        respondAsync
            gameGuid
            (registerPlayer playerName deck)
            (fun (player, newGame) ->
                updateGame newGame
                Success (player.Guid)
            )
            (Error ("Cannot register player"))

    [<Remote>]
    let GetPlayer (playerGuid : Guid) (gameGuid : Guid) =
        respondAsync
            gameGuid
            (getPlayer playerGuid)
            (fun player -> Success(player))
            (Error ("Cannot get player"))

    [<Remote>]
    let GetActivePlayerGuid (gameGuid : Guid) =
        respondAsync
            gameGuid
            (fun game ->
                Some game.ActivePlayerGuid
            )
            (fun guid -> Success(guid))
            (Error("Cannot get active player"))

    [<Remote>]
    let GetMulligan (playerGuid : Guid) (gameGuid : Guid) =
        respondAsync
            gameGuid
            (fun game ->
                getMulligan playerGuid game
            )
            (fun cardIdList -> Success(cardIdList))
            (Error("Cannot mulligan"))

    [<Remote>]
    let ReturnMulligan (cardIdList : string list) (playerGuid : Guid) (gameGuid : Guid) =
        respondAsync
            gameGuid
            (fun game ->
                match endMulligan cardIdList playerGuid game with
                | Some newGame -> 
                    updateGame newGame
                    match afterMulligan newGame with
                    | Some newGame2 ->
                        updateGame newGame2
                        Some newGame2
                    | None ->
                        Some newGame
                | None -> None
            )
            (fun newGame -> Success(newGame.CurrentPhase = Playing))
            (Error ("Cannot end mulligan"))
    // #endregion

    // #region Gameplay functions
    [<Remote>]
    let DrawCard (playerGuid : Guid) (gameGuid : Guid) =
        respondAsync
            gameGuid
            (fun game ->
                let success, newGame = drawCard playerGuid game
                match success with
                | Some card-> Some (card, newGame)
                | None -> None
            )
            (fun (newCard, newGame) ->
                updateGame newGame
                Success(newCard)
            )
            (Error("Cannot draw card"))

    [<Remote>]
    let DoesHeroPowerNeedTarget (heroPowerName : string) =
        async {
            match Hero.heroPowers |> List.tryFind(fun e -> e.Name = heroPowerName) with
            | Some heroPower -> return Success heroPower.Target.IsSome
            | None-> return Error("Cannot find hero power")
        }

    [<Remote>]
    let FindTargetForHeroPower (playerGuid : Guid) (gameGuid : Guid) =
        respondAsync
            gameGuid
            (fun game ->
                findTargetForHeroPower playerGuid game
            )
            (fun targetList -> Success(targetList))
            (Error("Cannot find target"))

    [<Remote>]
    let UseHeroPower (playerGuid : Guid) (targetGuid : Guid option) (gameGuid : Guid) =
        respondAsync
            gameGuid
            (fun game ->
                if targetGuid.IsSome then
                    findIChar targetGuid.Value game (fun _ -> ()) (fun _ -> ())
                    |> Option.bind(fun target ->
                        useHeroPower playerGuid (Some target) game
                        )
                else
                    useHeroPower playerGuid None game
            )
            (fun newGame ->
                updateGame newGame
                Success("Successfully use hero power")
            )
            (Error("Cannot use hero power"))
        
    [<Remote>]
    let StartGame (gameGuid : Guid) =
        respondAsync
            gameGuid
            (fun game -> startGame game)
            (fun newGame -> 
                updateGame newGame
                Success("Game started")
            )
            (Error("Cannot start game"))

    [<Remote>]
    let EndTurn (playerGuid : Guid) (gameGuid : Guid) =
        respondAsync
            gameGuid
            (fun game -> 
                if playerGuid <> game.ActivePlayerGuid then None
                else endTurn game)
            (fun newGame ->
                updateGame newGame
                Success("Turn ended")
            )
            (Error ("Current player is not active or current phase is not Playing"))

    [<Remote>]
    let DoesCardNeedTarget (cardId : string) =
        async {
            match getTargetForCard cardId with
            | Some target, _-> return true
            | None, _ -> return false
        }

    [<Remote>]
    let FindTargetForCard (card : Card) (playerGuid : Guid) (gameGuid : Guid) =
        respondAsync
            gameGuid
            (fun game ->
                findTargetForCard card playerGuid game
            )
            (fun targetList -> Success(targetList))
            (Error("Cannot find target"))

    [<Remote>]
    let PlayCard (card : CardOnHand) (pos : int option) (targetGuid : Guid option) (playerGuid : Guid) (gameGuid : Guid) =
        respondAsync
            gameGuid
            (fun game ->
                getPlayer playerGuid game
                |> Option.bind(fun player -> 
                    if targetGuid.IsSome then
                        findIChar targetGuid.Value game (fun _ -> ()) (fun _ -> ())
                        |> Option.bind(fun target ->
                            playCard card pos (Some target) playerGuid game
                            )
                    else
                        playCard card pos None playerGuid game
                )
            )
            (fun newGame -> 
                updateGame newGame
                Success("Played: " + card.Card.Name)
            )
            (Error("Cannot play card"))

    [<Remote>]
    let FindTargetToAttack (playerGuid : Guid) (gameGuid : Guid) =
        respondAsync
            gameGuid
            (fun game ->
                findTargetToAttack playerGuid game
            )
            (fun targetList -> 
                Success(targetList))
            (Error("Cannot find target"))

    [<Remote>]
    let AttackIChar (source : Guid) (target : Guid) (gameGuid : Guid) =
        respondAsync
            gameGuid
            (fun game ->
                findIChar source game (fun _ -> ()) (fun _ -> ())
                    |> Option.bind(fun sourceIChar ->
                        findIChar target game (fun _ -> ()) (fun _ -> ())
                        |> Option.bind(fun targetIChar ->
                            attackIChar sourceIChar targetIChar game
                        )
                    )
            )
            (fun newGame -> 
                updateGame newGame
                Success("Attacked")
            )
            (Error("Cannot attack"))
    // #endregion