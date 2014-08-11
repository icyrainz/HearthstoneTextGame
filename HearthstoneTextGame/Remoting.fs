namespace HearthstoneTextGame

open System.Collections.Generic
open IntelliFactory.WebSharper

open Game

type Response<'T> =
    | Success of 'T
    | Error of string

module Remoting =

    let gameDb = new Dictionary<Guid, System.DateTime * GameSession>()

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
        | false ->
            gameDb.Add(game.Guid, (now, game))

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
    let GetPlayableClass () = Hero.playableClasses

    [<Remote>]
    let GetPredefinedDeck () = 
        async {
            return Deck.PredefinedDecks
        }

    [<Remote>]
    let GetPlayer (playerGuid : Guid) (gameGuid : Guid) =
        respondAsync
            gameGuid
            (getPlayer playerGuid)
            (fun player -> Success(player))
            (Error ("Cannot get player"))

    [<Remote>]
    let DrawCard (playerGuid : Guid) (gameGuid : Guid) =
        respondAsync
            gameGuid
            (fun game ->
                getPlayer playerGuid game
                |> Option.bind(fun player ->
                    let success, newGame = drawCard player game
                    match success with
                    | Some card-> Some (card, newGame)
                    | None -> None)
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
    let UseHeroPower (playerGuid : Guid) (targetGuid : Guid option) (gameGuid : Guid) =
        respondAsync
            gameGuid
            (fun game ->
                if targetGuid.IsSome then
                    findIChar targetGuid.Value game (fun _ -> ()) (fun _ -> ())
                    |> Option.bind(fun target ->
                        getPlayer playerGuid game
                        |> Option.bind(fun player -> useHeroPower player (Some target) game)
                        )
                else
                    getPlayer playerGuid game
                    |> Option.bind(fun player -> useHeroPower player None game)
            )
            (fun newGame ->
                updateGame newGame
                Success("Successfully use hero power")
            )
            (Error("Cannot use hero power"))

    [<Remote>]
    let FindTargetForHeroPower (playerGuid : Guid) (gameGuid : Guid) =
        respondAsync
            gameGuid
            (fun game ->
                getPlayer playerGuid game
                |> Option.bind(fun player -> findTargetForHeroPower player game)
            )
            (fun targetList -> Success(targetList))
            (Error("Cannot find target"))

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
                        |> List.tryFind(fun e -> e.HeroCharacter.Guid = guid)
                        |> Option.iter(fun player -> name := player.Name)
                    )
                    (fun minion -> name := minion.Card.Name) |> ignore
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
    let GetActivePlayerGuid (gameGuid : Guid) =
        respondAsync
            gameGuid
            (fun game ->
                Some game.ActivePlayerGuid
            )
            (fun guid -> Success(guid))
            (Error("Cannot get active player"))
        
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
    let GetGameLastChangedTime (gameGuid : Guid) =
        async {
            match getGameTime (gameGuid) with
            | Some time -> return Success(time.Ticks)
            | None -> return Error("Cannot get game time")
        }
        |> Async.RunSynchronously

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
    let DoesCardNeedTarget (cardName : string) =
        async {
            match Card.getTargetForCard cardName with
            | Some target -> return true
            | None -> return false
        }

    [<Remote>]
    let FindTargetForCard (card : Card) (playerGuid : Guid) (gameGuid : Guid) =
        respondAsync
            gameGuid
            (fun game ->
                getPlayer playerGuid game
                |> Option.bind(fun player -> findTargetForCard card player game)
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
                            getPlayer playerGuid game
                            |> Option.bind(fun player -> playCard card pos (Some target) player game)
                            )
                    else
                        getPlayer playerGuid game
                            |> Option.bind(fun player -> playCard card pos None player game)
                )
            )
            (fun newGame -> 
                updateGame newGame
                Success("Played: " + card.Card.Name)
            )
            (Error("Cannot play card"))