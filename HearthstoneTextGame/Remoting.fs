namespace HearthstoneTextGame

open System
open System.Collections.Generic
open IntelliFactory.WebSharper

open Game

type Response<'T> =
    | Success of 'T
    | Error of string

module Remoting =

    let gameDb = new Dictionary<string, GameSession>()

    let updateGame (game : GameSession) =
        match gameDb.ContainsKey(game.Guid) with
        | true ->
            gameDb.Remove(game.Guid) |> ignore
            gameDb.Add(game.Guid, game)
        | false ->
            gameDb.Add(game.Guid, game)

    let doesGameExist gameGuid = 
        let isExisted, game = gameDb.TryGetValue(gameGuid)
        match isExisted with
        | true -> Some game
        | false -> None

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
                            ifFailure)
        }

    [<Remote>]
    let NewGame () =
        async {
            try
                let newGame = GameSession.Init()
                gameDb.Add(newGame.Guid, newGame)
                return Success(newGame.Guid)
            with
            | :? System.ArgumentNullException -> return Error("Invalid game guid")
            | :? System.ArgumentException -> return Error("Game already exists")    
        }

    [<Remote>]
    let RegisterPlayer (playerName : string) (gameGuid : string) =
        respondAsync
            gameGuid
            (registerRandomDeckPlayer playerName)
            (fun (player, newGame) ->
                updateGame newGame
                Success (player.Guid))
            (Error ("Cannot register player"))

    [<Remote>]
    let RegisterPlayerWithClass (playerName : string) (playerClass : string) (gameGuid : string) =
        respondAsync
            gameGuid
            (registerRandomDeckPlayerWithClass playerName playerClass)
            (fun (player, newGame) ->
                updateGame newGame
                Success (player.Guid))
            (Error ("Cannot register player"))

    [<Remote>]
    let GetPlayableClass () = Hero.playableClasses

    [<Remote>]
    let GetPlayer (playerGuid : string) (gameGuid : string) =
        respondAsync
            gameGuid
            (getPlayer playerGuid)
            (fun player -> Success(player))
            (Error ("Cannot get player"))

    [<Remote>]
    let DrawCard (playerGuid : string) (gameGuid : string) =
        respondAsync
            gameGuid
            (fun game ->
                getPlayer playerGuid game
                |> Option.bind(fun player -> drawCard player game)
            )
            (fun (newCard, newGame) ->
                updateGame newGame
                Success(Card.getCardById newCard))
            (Error("Cannot draw card"))

    [<Remote>]
    let DoesHeroPowerNeedTarget (heroPowerName : string) =
        async {
            match Hero.heroPowers |> List.tryFind(fun e -> e.Name = heroPowerName) with
            | Some heroPower -> return Success heroPower.Target.IsSome
            | None-> return Error("Cannot find hero power")
        }

    [<Remote>]
    let UseHeroPower (playerGuid : string) (targetGuid : string option) (gameGuid : string) =
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
                Success("Successfully use hero power"))
            (Error("Cannot use hero power"))

    [<Remote>]
    let FindTargetForHeroPower (playerGuid : string) (gameGuid : string) =
        respondAsync
            gameGuid
            (fun game ->
                getPlayer playerGuid game
                |> Option.bind(fun player -> findTargetForHeroPower player game)
            )
            (fun targetList -> Success(targetList))
            (Error("Cannot find target"))

    [<Remote>]
    let GetICharName (guid : string) (gameGuid : string) =
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