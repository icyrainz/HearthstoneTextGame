namespace HearthstoneTextGame

open System
open IntelliFactory.WebSharper
open IntelliFactory.WebSharper.Html
open IntelliFactory.WebSharper.JQuery

[<JavaScript>]
module Client =

    let NewGame callback =
        async {
            let! result = Remoting.NewGame()
            return callback result
        }
        |> Async.Start

    let doAsync doSth callback =
        async {
            let! result = doSth
            return callback result
        }
        |> Async.Start

    type Style =
        | Green
        | Default

    let btn text (style : Style) = 
        let styleClass =
            match style with
            | Green -> "success"
            | Default -> "default"
        Button [Attr.Type "button"; Attr.Class ("btn btn-block btn-" + styleClass); Text text]

    type GameClient () =
        member val Guid = JavaScript.Undefined<string> with get, set
        member val LastChanged = JavaScript.Undefined<int64> with get, set
        member val ActivePlayerGuid = JavaScript.Undefined<string> with get, set
        member val LeftPlayer = JavaScript.Undefined<Player> with get, set
        member val RightPlayer = JavaScript.Undefined<Player> with get, set
        member __.Exist () = __.Guid <> JavaScript.Undefined<string>
        member __.HasLeftPlayer () = __.LeftPlayer <> JavaScript.Undefined<Player>
        member __.HasRightPlayer () = __.RightPlayer <> JavaScript.Undefined<Player>
        member __.Clear() =
            __.Guid <- JavaScript.Undefined<string>
            __.LastChanged <- JavaScript.Undefined<int64> 
            __.LeftPlayer <- JavaScript.Undefined<Player>
            __.RightPlayer <- JavaScript.Undefined<Player>

    [<Inline " updatePopover() ">]
    let updatePopover = ()

    let Main () =

        let currentGame = GameClient()
        let gameGuidLabel = Span [Text "[None]"]

        let newGameButton = btn "NewGame" Green
        let registerPlayerButton = btn "Register" Default
        let startGameButton = btn "Start Game" Default

        let leftPlayerEndTurnButton = btn "End Turn" Default
        let leftPlayerUseHeroPowerButton = btn "Use Hero Power" Default
        let leftPlayerHand = UL [Attr.Class "list-group"]
//        let leftPlayerdrawCardButton = Button [Attr.Type "button"; Attr.Class "btn btn-default btn-block"; Text "DrawCard"]
        let leftPlayerBoard = UL [Attr.Class "list-group"]
        let leftPlayerManaBar = 
            Div [Attr.Class "progress"] -< [
                Div [Attr.Class "progress-bar progress-bar-striped"
                     Attr.NewAttr "role" "progressbar"
                     Attr.Style "width: 0%;"
                     Id "proggressbarLunused"]
                Div [Attr.Class "progress-bar progress-bar-warning"
                     Attr.NewAttr "role" "progressbar"
                     Attr.Style "width: 0%;"
                     Id "proggressbarLused"]
            ]
        let leftPlayerInfoTable = 
            Table [Attr.Class "table table-hover"; Id "infoLeft"] -< [
                THead [] -< [
                    TR [] -< [
                        TH [Text "field"]
                        TH [Text "value"]
                    ]
                ]
                TBody [] -< [
                    TR [] -< [
                        TD [Text "Player Name"]
                        TD [Id "leftPlayerName"; Text "[None]"]
                    ]
                    TR [] -< [
                        TD [Text "Player Class"]
                        TD [Id "leftPlayerClass"; Text "[None]"]
                    ]
                    TR [] -< [
                        TD [Text "Hero Power"]
                        TD [Id "leftPlayerHeroPower"; Text "[None]"]
                    ]
                    TR [] -< [
                        TD [Text "Remaining Cards"]
                        TD [Id "leftPlayerRemainingCardsCount"; Text "[None]"]
                    ]
                    TR [] -< [
                        TD [Text "Health"]
                        TD [Id "leftPlayerHealth"; Text "[None]"]
                    ]
                ]
            ]

        let rightPlayerEndTurnButton = btn "End Turn" Default
        let rightPlayerHand = UL [Attr.Class "list-group"]
//        let rightPlayerdrawCardButton = Button [Attr.Type "button"; Attr.Class "btn btn-default btn-block"; Text "DrawCard"]
        let rightPlayerBoard = UL [Attr.Class "list-group"]
        let rightPlayerManaBar = 
            Div [Attr.Class "progress"] -< [
                Div [Attr.Class "progress-bar progress-bar-striped"
                     Attr.NewAttr "role" "progressbar"
                     Attr.Style "width: 0%;"
                     Id "proggressbarRunused"]
                Div [Attr.Class "progress-bar progress-bar-warning"
                     Attr.NewAttr "role" "progressbar"
                     Attr.Style "width: 0%;"
                     Id "proggressbarRused"]
            ]
        let rightPlayerInfoTable = 
            Table [Attr.Class "table table-hover"; Id "infoRight"] -< [
                THead [] -< [
                    TR [] -< [
                        TH [Text "field"]
                        TH [Text "value"]
                    ]
                ]
                TBody [] -< [
                    TR [] -< [
                        TD [Text "Player Name"]
                        TD [Id "rightPlayerName"; Text "[None]"]
                    ]
                    TR [] -< [
                        TD [Text "Player Class"]
                        TD [Id "rightPlayerClass"; Text "[None]"]
                    ]
                    TR [] -< [
                        TD [Text "Hero Power"]
                        TD [Id "rightPlayerHeroPower"; Text "[None]"]
                    ]
                    TR [] -< [
                        TD [Text "Remaining Cards"]
                        TD [Id "rightPlayerRemainingCardsCount"; Text "[None]"]
                    ]
                    TR [] -< [
                        TD [Text "Health"]
                        TD [Id "rightPlayerHealth"; Text "[None]"]
                    ]
                ]
            ]
        let askItems = Div []
        let askModalContentDiv =
            Div [] -< [ UL [Attr.Class "list-group"] -- askItems ]

        let saveItemButton = 
            Button [Attr.Type "button"
                    Attr.Class "btn btn-primary"
                    Attr.NewAttr "data-dismiss" "modal"
                    Text "Save"]

        let openModalButton =
            Button [Attr.NewAttr "data-toggle" "modal"
                    Attr.NewAttr "data-target" "#askModal"]

        let modalDiv =
            Div [Attr.Class "modal fade"
                 Attr.Id "askModal"
                 Attr.NewAttr "tab-index" "-1"
                 Attr.NewAttr "role" "dialog"
                 Attr.NewAttr "aria-labelledby" "askModalLabel"
                 Attr.NewAttr "aria-hidden" "true"] -< [
                Div [Attr.Class "modal-dialog modal-sm"] -< [
                    Div [Attr.Class "modal-content"] -< [
                        Div [Attr.Class "modal-header"] -< [
                            Button [Attr.Type "button"
                                    Attr.Class "close"
                                    Attr.NewAttr "data-dismiss" "modal"] -< [
                                Span [Attr.NewAttr "aria-hidden" "true"; Text "x"]
                                Span [Attr.Class "sr-only"; Text "Close"]            
                            ]
                            H4 [Attr.Class "modal-title"; Id "askModalLabel"; Text "Choose"]
                        ]
                        Div [Attr.Class "modal-body"] -- askModalContentDiv
                        Div [Attr.Class "modal-footer"] -< [
                            Button [Attr.Type "button"
                                    Attr.Class "btn btn-default"
                                    Attr.NewAttr "data-dismiss" "modal"
                                    Text "Close"]
                            saveItemButton
                        ]
                    ]
                ]
            ]

        let cardTemplateDiv cardName cardId =
            let newItem = LI [Attr.Class "list-group-item"]

            let cardImgUrl = "http://wow.zamimg.com/images/hearthstone/cards/enus/small/" + cardId + ".png"
            let previewButton =
                JQuery.Of("<button />")
                    .Attr("type", "button")
                    .AddClass("btn")
                    .AddClass("btn-default")
                    .Attr("data-toggle", "popover")
                    .Attr("rel", "popover")
                    .Attr("data-img", cardImgUrl)
                    .Attr("data-default-title", "")
                    .Text("Image")
            let playCardButton = 
                JQuery.Of("<button />")
                    .Attr("type", "button")
                    .AddClass("btn")
                    .AddClass("btn-default")
                    .Text("Play")
                    .Click(fun _ _ ->
                        ()
                    )
            let cardNameSpan =
                JQuery.Of("<h5/>").Text(cardName)

            JQuery.Of(newItem.Dom)
                .Append(JQuery.Of("<div />").AddClass("row").AddClass("clearfix")
                    .Append(JQuery.Of("<div />").AddClass("col-xs-4").Append(previewButton).Append(playCardButton))
                    .Append(JQuery.Of("<div />").AddClass("col-xs-8").Append(cardNameSpan))).Ignore
            newItem

        let clearGame () =
            currentGame.Clear()
            JQuery.Of("[id^=leftPlayer]").Text("[None]").Ignore
            JQuery.Of(leftPlayerHand.Dom).Children().Remove().Ignore
            JQuery.Of("#infoLeft").Css("background-color", "").Ignore
//            JQuery.Of(leftPlayerdrawCardButton.Dom).FadeOut().Ignore
            JQuery.Of(leftPlayerEndTurnButton.Dom)
                    .RemoveClass("btn-success")
                    .RemoveClass("btn-warning")
                    .Attr("disabled", "true").Ignore

            JQuery.Of("[id^=rightPlayer]").Text("[None]").Ignore
            JQuery.Of(rightPlayerHand.Dom).Children().Remove().Ignore
            JQuery.Of("#infoRight").Css("background-color", "").Ignore
//            JQuery.Of(rightPlayerdrawCardButton.Dom).FadeOut().Ignore
            JQuery.Of(rightPlayerEndTurnButton.Dom)
                .RemoveClass("btn-success")
                .RemoveClass("btn-warning")
                .Attr("disabled", "true").Ignore

        let newGame gameGuid =
            clearGame()
            currentGame.Guid <- gameGuid
            JQuery.Of(gameGuidLabel.Dom).Text(gameGuid + " " + (string currentGame.LastChanged)).Ignore
            match Remoting.GetGameLastChangedTime currentGame.Guid with
            | Success time -> currentGame.LastChanged <- time
            | Error msg -> JavaScript.Alert(msg)

        let updateMana now max playerGuid =
            JQuery.Of("#proggressbarLunused").RemoveClass("active").Ignore
            JQuery.Of("#proggressbarRunused").RemoveClass("active").Ignore
            if playerGuid = currentGame.LeftPlayer.Guid then
                JQuery.Of("#proggressbarLunused")
                    .AddClass("active")
                    .Css("width", (int(now) * 10).ToString() + "%").Ignore
                JQuery.Of("#proggressbarLused")
                    .Css("width", ((int(max) - int(now)) * 10).ToString() + "%").Ignore
            else
                JQuery.Of("#proggressbarRunused")
                    .AddClass("active")
                    .Css("width", (int(now) * 10).ToString() + "%").Ignore
                JQuery.Of("#proggressbarRused")
                    .Css("width", ((int(max) - int(now)) * 10).ToString() + "%").Ignore

        let updateLeftPlayer (player) =
            currentGame.LeftPlayer <- player
            let isActive = currentGame.LeftPlayer.Guid = currentGame.ActivePlayerGuid
            if isActive then
                JQuery.Of("#infoLeft").Css("background-color", "lightyellow").Ignore
                JQuery.Of(leftPlayerEndTurnButton.Dom)
                    .RemoveClass("btn-success")
                    .AddClass("btn-warning")
                    .RemoveAttr("disabled").Ignore
            else
                JQuery.Of("#infoLeft").Css("background-color", "").Ignore
                JQuery.Of(leftPlayerEndTurnButton.Dom)
                    .RemoveClass("btn-success")
                    .RemoveClass("btn-warning")
                    .Attr("disabled", "true").Ignore

            JQuery.Of("#leftPlayerName").Text(currentGame.LeftPlayer.Name).Ignore
            JQuery.Of("#leftPlayerClass").Text(currentGame.LeftPlayer.HeroClass).Ignore
            JQuery.Of("#leftPlayerHeroPower").Text(currentGame.LeftPlayer.HeroPower.Name).Ignore
            match (not currentGame.LeftPlayer.HeroPowerUsed) && isActive with
            | true -> JQuery.Of(leftPlayerUseHeroPowerButton.Dom).AddClass("btn-success").RemoveAttr("disabled").Ignore
            | false -> JQuery.Of(leftPlayerUseHeroPowerButton.Dom).RemoveClass("btn-success").Attr("disabled", "true").Ignore
            JQuery.Of("#leftPlayerRemainingCardsCount").Text(string <| List.length currentGame.LeftPlayer.Deck.CardIdList).Ignore
            JQuery.Of("#leftPlayerHealth").Text(currentGame.LeftPlayer.HeroCharacter.Hp.ToString()).Ignore
//            JQuery.Of(leftPlayerdrawCardButton.Dom).FadeIn().Ignore
            
            updateMana currentGame.LeftPlayer.CurrentMana currentGame.LeftPlayer.MaxMana currentGame.LeftPlayer.Guid

            JQuery.Of(leftPlayerHand.Dom).Children().Remove().Ignore
            currentGame.LeftPlayer.Hand |> List.iter(fun card -> cardTemplateDiv card.Card.Name card.Card.Id |> leftPlayerHand.Append)
            updatePopover

        let updateRightPlayer (player) =
            currentGame.RightPlayer <- player
            let isActive = currentGame.RightPlayer.Guid = currentGame.ActivePlayerGuid
            if isActive then
                JQuery.Of("#infoRight").Css("background-color", "lightyellow").Ignore
                JQuery.Of(rightPlayerEndTurnButton.Dom)
                    .RemoveClass("btn-success")
                    .AddClass("btn-warning")
                    .RemoveAttr("disabled").Ignore
            else
                JQuery.Of("#infoRight").Css("background-color", "").Ignore
                JQuery.Of(rightPlayerEndTurnButton.Dom)
                    .RemoveClass("btn-success")
                    .RemoveClass("btn-warning")
                    .Attr("disabled", "true").Ignore

            JQuery.Of("#rightPlayerName").Text(currentGame.RightPlayer.Name).Ignore
            JQuery.Of("#rightPlayerClass").Text(currentGame.RightPlayer.HeroClass).Ignore
            JQuery.Of("#rightPlayerHeroPower").Text(currentGame.RightPlayer.HeroPower.Name).Ignore
            JQuery.Of("#rightPlayerRemainingCardsCount").Text(string <| List.length currentGame.RightPlayer.Deck.CardIdList).Ignore
            JQuery.Of("#rightPlayerHealth").Text(currentGame.RightPlayer.HeroCharacter.Hp.ToString()).Ignore
//            JQuery.Of(rightPlayerdrawCardButton.Dom).FadeIn().Ignore
            
            updateMana currentGame.RightPlayer.CurrentMana currentGame.RightPlayer.MaxMana currentGame.RightPlayer.Guid

            JQuery.Of(rightPlayerHand.Dom).Children().Remove().Ignore
            currentGame.RightPlayer.Hand |> List.iter(fun card -> cardTemplateDiv card.Card.Name card.Card.Id |> rightPlayerHand.Append)
            updatePopover

        let updatePlayer playerGuid =
            doAsync (Remoting.GetPlayer playerGuid currentGame.Guid)
                (fun res -> 
                    match res with
                    | Success player -> 
                        if (not <| currentGame.HasLeftPlayer()) || currentGame.LeftPlayer.Guid = player.Guid then
                            updateLeftPlayer player
                        else if (not <| currentGame.HasRightPlayer()) || currentGame.RightPlayer.Guid = player.Guid then
                            updateRightPlayer player
                    | Error msg -> JavaScript.Alert(msg))
        
        let updatePlayers () =
            if currentGame.Exist() then
                match Remoting.GetGameLastChangedTime currentGame.Guid with
                | Error msg -> JavaScript.Log(msg)
                | Success time ->
                    if time > currentGame.LastChanged then
                        doAsync (Remoting.GetActivePlayerGuid(currentGame.Guid))
                            (fun ret -> match ret with
                                        | Success guid -> currentGame.ActivePlayerGuid <- guid
                                        | Error msg -> JavaScript.Alert(msg)
                            )
                        [ if currentGame.HasLeftPlayer() then yield currentGame.LeftPlayer.Guid
                          if currentGame.HasRightPlayer() then yield currentGame.RightPlayer.Guid ]
                        |> List.iter(fun e -> updatePlayer e)
                        currentGame.LastChanged <- time
                        JQuery.Of(gameGuidLabel.Dom).Text(currentGame.Guid + " " + (string currentGame.LastChanged)).Ignore

        let playCard (cardId : string) (player : Player) =
            ()

        let addItemsToAsk (items : string list) (callback : string -> unit) =
            JQuery.Of(askItems.Dom).Empty().Ignore
            items |> List.iter(fun e ->
                let newItem = LI [Attr.Class "list-group-item"; Text e]
                askItems.Append(newItem)
                newItem |>! OnClick (fun evt m ->
                    JQuery.Of(askItems.Dom).Children("li").RemoveClass("active").Ignore
                    JQuery.Of(saveItemButton.Dom).Unbind("click").Ignore

                    JQuery.Of(evt.Dom).AddClass("active").Ignore
                    JQuery.Of(saveItemButton.Dom).Click(fun _ _ -> callback evt.Text).Ignore
                ) |> ignore)
            JQuery.Of(openModalButton.Dom).Click().Ignore

        let backgroundTask = 
            JavaScript.SetInterval
                (fun _ ->
                    updatePlayers()
                )
                1000

        let setupButton =
            
            JQuery.Of(startGameButton.Dom)
                .Click(fun _ _ ->
                    doAsync (Remoting.StartGame currentGame.Guid)
                        (fun ret ->
                            match ret with
                            | Success msg -> JavaScript.Alert(msg)
                            | Error msg -> JavaScript.Alert(msg)
                        )
                ).Ignore
            
            JQuery.Of(newGameButton.Dom)
                .Click(fun _ _ ->
                    NewGame (fun res ->
                        match res with
                        | Success gameGuid -> newGame gameGuid
                        | Error msg -> JavaScript.Alert(msg))).Ignore

            JQuery.Of(registerPlayerButton.Dom)
                .Click(fun _ _ ->
                    let playerName =
                        if not <| currentGame.HasLeftPlayer() then
                            Some "LeftPlayer"
                        else if not <| currentGame.HasRightPlayer() then
                            Some "RightPlayer"
                        else
                            None
                    match playerName with
                    | Some name ->
                        let classList = Remoting.GetPlayableClass()
                        addItemsToAsk classList (fun selection ->
                            doAsync (Remoting.RegisterPlayerWithClass name selection currentGame.Guid) 
                                (fun res ->
                                    match res with
                                    | Success playerGuid -> updatePlayer playerGuid
                                    | Error msg -> JavaScript.Alert(msg))
                        )
                    | None -> ()).Ignore

//            JQuery.Of(leftPlayerdrawCardButton.Dom)
//                .Click(fun _ _ ->
//                    doAsync (Remoting.DrawCard currentGame.LeftPlayer.Guid currentGame.Guid)
//                        (fun res ->
//                            match res with
//                            | Success card ->
//                                updatePlayer currentGame.LeftPlayer.Guid
//                            | Error msg ->
//                                JavaScript.Alert(msg))).Hide().Ignore
//
//            JQuery.Of(rightPlayerdrawCardButton.Dom)
//                .Click(fun _ _ ->
//                    doAsync (Remoting.DrawCard (!rightPlayer).Guid currentGame.Guid)
//                        (fun res ->
//                            match res with
//                            | Success card ->
//                                updatePlayer (!rightPlayer).Guid
//                            | Error msg ->
//                                JavaScript.Alert(msg))).Hide().Ignore

            JQuery.Of(openModalButton.Dom).Hide().Ignore

            JQuery.Of(leftPlayerEndTurnButton.Dom).Attr("disabled", "true")
                .Click(fun _ _ ->
                    doAsync (Remoting.EndTurn currentGame.LeftPlayer.Guid currentGame.Guid)
                        (fun ret ->
                            match ret with
                            | Success msg -> JavaScript.Log(msg)
                            | Error msg -> JavaScript.Log(msg) 
                        )
                    )
                .Ignore

            JQuery.Of(rightPlayerEndTurnButton.Dom).Attr("disabled", "true")
                .Click(fun _ _ ->
                    doAsync (Remoting.EndTurn currentGame.RightPlayer.Guid currentGame.Guid)
                        (fun ret ->
                            match ret with
                            | Success msg -> JavaScript.Log(msg)
                            | Error msg -> JavaScript.Log(msg) 
                        )
                    )
                .Ignore
            
            
            JQuery.Of(leftPlayerUseHeroPowerButton.Dom)
                .Attr("disabled", "true")
                .Click(fun _ _ ->
                    doAsync (Remoting.DoesHeroPowerNeedTarget (currentGame.LeftPlayer.HeroPower.Name))
                        (fun needTarget ->
                            match needTarget with
                            | Success true ->
                                doAsync (Remoting.FindTargetForHeroPower currentGame.LeftPlayer.Guid currentGame.Guid)
                                    (fun res ->
                                        match res with
                                        | Success items ->
                                            let newItems =
                                                items |> List.map (fun item ->
                                                    match Remoting.GetICharName (item) currentGame.Guid with
                                                    | Success name -> name
                                                    | Error msg -> "Fail to load name !"
                                                )
                                            let itemNameWithGuid = List.zip newItems items
                                            addItemsToAsk newItems (fun choice ->
                                                let selGuid = itemNameWithGuid |> List.find(fun (name, guid) -> name = choice) |> snd
                                                doAsync (Remoting.UseHeroPower currentGame.LeftPlayer.Guid (Some selGuid) currentGame.Guid)
                                                    (fun afterUse ->
                                                        match afterUse with
                                                        | Success msg -> JavaScript.Alert(msg)
                                                        | Error msg -> JavaScript.Alert(msg)
                                                    )
                                            )
                                        | Error msg -> ()
                                    )
                            | Success false ->
                                doAsync (Remoting.UseHeroPower currentGame.LeftPlayer.Guid None currentGame.Guid)
                                    (fun res ->
                                        match res with
                                        | Success msg -> JavaScript.Alert(msg)
                                        | Error msg -> JavaScript.Alert(msg)
                                    )
                            | Error msg -> JavaScript.Alert(msg))).Ignore

        Div [Attr.Class "col-md-12"] -< [

            Div [Attr.Class "panel panel-primary"] -< [
                Div [Attr.Class "panel-heading"] -- H3 [Attr.Class "panel-title"; Text "Game Control"]
                Div [Attr.Class "panel-body"] -< [
                    Div [Attr.Class "row clearfix"] -< [
                        Div [Attr.Class "col-sm-4"] -- newGameButton
                        Div [Attr.Class "col-sm-4"] -- registerPlayerButton
                        Div [Attr.Class "col-sm-4"] -- startGameButton
                    ]
                ]
                Div [Attr.Class "panel-footer"] -< [
                    Div [Attr.Class "row clearfix"] -< [
                        H4 [Attr.Class "col-sm-2"] -< [
                            Span [Attr.Class "label label-info center-block"; Text "Game Guid"]
                        ]
                        H4 [Attr.Class "col-sm-10 text-center"] -- gameGuidLabel
                    ]
                ]
            ]

            Div [Attr.Class "row clearfix"] -< [
                Div [Attr.Class "col-md-6 column"] -< [
                    Div [Attr.Class "panel panel-default"] -< [
                        Div [Attr.Class "panel-heading"] -- H3 [Attr.Class "panel-title"; Text "Left Player"]
                        Div [Attr.Class "panel-body"] -< [ 
//                            leftPlayerdrawCardButton
                            leftPlayerEndTurnButton
                            leftPlayerUseHeroPowerButton
                            leftPlayerInfoTable
                            leftPlayerManaBar
                        ]
                    ]
                    leftPlayerHand
                ]
                Div [Attr.Class "col-md-6 column"] -< [
                    Div [Attr.Class "panel panel-default"] -< [
                        Div [Attr.Class "panel-heading"] -- H3 [Attr.Class "panel-title"; Text "Right Player"]
                        Div [Attr.Class "panel-body"] -< [ 
//                            rightPlayerdrawCardButton
                            rightPlayerEndTurnButton
                            rightPlayerInfoTable
                            rightPlayerManaBar
                        ]
                    ]
                    rightPlayerHand
                ]
            ]
            Div [Attr.Class "panel panel-info"] -< [
                Div [Attr.Class "panel-heading"] -- H3 [Attr.Class "panel-title"; Text "Game Board"]
                Div [Attr.Class "panel-body"] -< [
                    Div [Attr.Class "row clearfix"] -< [
                        Div [Attr.Class "col-md-6"] -- leftPlayerBoard
                        Div [Attr.Class "col-md-6"] -- rightPlayerBoard
                    ]
                ]
            ]
            modalDiv
            openModalButton
        ]

    let About () =
        Div [Attr.Class "jumbotron"] -< [
            H3 [Text "@Tue AKIO 2014"]
        ]
