namespace HearthstoneTextGame

open System
open IntelliFactory.WebSharper
open IntelliFactory.WebSharper.Html
open IntelliFactory.WebSharper.JQuery

[<JavaScript>]
module Client =

    let Start input k =
        async {
            let! data = Remoting.Process(input)
            return k data
        }
        |> Async.Start

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

    let registerPlayer (playerName : string) (gameGuid : string) callback =
        doAsync (Remoting.RegisterPlayer playerName gameGuid) callback

    let registerPlayerWithClass (playerName : string) (playerClass : string) (gameGuid : string) callback =
        doAsync (Remoting.RegisterPlayerWithClass playerName playerClass gameGuid) callback

    let getPlayer (playerGuid : string) (gameGuid : string) callback =
        doAsync (Remoting.GetPlayer playerGuid gameGuid) callback

    let drawCard (playerGuid : string) (gameGuid : string) callback =
        doAsync (Remoting.DrawCard playerGuid gameGuid) callback

    let doesHeroPowerNeedTarget (heroPowerName : string) callback =
        doAsync (Remoting.DoesHeroPowerNeedTarget heroPowerName) callback

    let findTargetForHeroPower (playerGuid : string) (gameGuid : string) callback =
        doAsync (Remoting.FindTargetForHeroPower playerGuid gameGuid) callback

    let useHeroPower (playerGuid : string) (targetGuid : string option) (gameGuid : string) callback =
        doAsync (Remoting.UseHeroPower playerGuid targetGuid gameGuid) callback

    let getICharName (guid : string) (gameGuid : string) =
        Remoting.GetICharName guid gameGuid

    let getPlayableClasses () =
        Remoting.GetPlayableClass()

    let getCard (cardId : string) =
        Remoting.GetCard cardId

    [<Inline " updatePopover() ">]
    let updatePopover = ()

    let Main () =

        let currentGameGuid = ref ""
        let gameGuidLabel = Span [Text "[None]"]

        let newGameButton = Button [Attr.Type "button"; Attr.Class "btn btn-success btn-block"; Text "New Game"]
        let registerPlayerButton = Button [Attr.Type "button"; Attr.Class "btn btn-default btn-block"; Text "Register"]
        let testButton = Button [Attr.Type "button"; Attr.Class "btn btn-default btn-block"; Text "Test"]

        let leftPlayer = ref JavaScript.Undefined<Player>
        let leftPlayerUseHeroPowerButton = Button [Attr.Type "button"; Attr.Class "btn btn-default btn-block"; Text "Use Hero Power"]
        let leftPlayerHand = UL [Attr.Class "list-group"]
        let leftPlayerdrawCardButton = Button [Attr.Type "button"; Attr.Class "btn btn-default btn-block"; Text "DrawCard"]
        let leftPlayerBoard = UL [Attr.Class "list-group"]
        let leftPlayerManaBar = 
            Div [Attr.Class "progress"] -< [
                Div [Attr.Class "progress-bar progress-bar-striped active"
                     Attr.NewAttr "role" "progressbar"
                     Attr.Style "width: 0%;"
                     Id "proggressbarLunused"]
                Div [Attr.Class "progress-bar progress-bar-warning"
                     Attr.NewAttr "role" "progressbar"
                     Attr.Style "width: 0%;"
                     Id "proggressbarLused"]
            ]
        let leftPlayerInfoTable = 
            Table [Attr.Class "table table-hover"] -< [
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


        let rightPlayer = ref JavaScript.Undefined<Player>
        let rightPlayerHand = UL [Attr.Class "list-group"]
        let rightPlayerdrawCardButton = Button [Attr.Type "button"; Attr.Class "btn btn-default btn-block"; Text "DrawCard"]
        let rightPlayerBoard = UL [Attr.Class "list-group"]
        let rightPlayerManaBar = 
            Div [Attr.Class "progress"] -< [
                Div [Attr.Class "progress-bar progress-bar-striped active"
                     Attr.NewAttr "role" "progressbar"
                     Attr.Style "width: 0%;"
                     Id "proggressbarRunused"]
                Div [Attr.Class "progress-bar progress-bar-warning"
                     Attr.NewAttr "role" "progressbar"
                     Attr.Style "width: 0%;"
                     Id "proggressbarRused"]
            ]
        let rightPlayerInfoTable = 
            Table [Attr.Class "table table-hover"] -< [
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
            JQuery.Of("[id^=leftPlayer]").Text("[None]").Ignore
            JQuery.Of(leftPlayerHand.Dom).Children().Remove().Ignore
            JQuery.Of(leftPlayerdrawCardButton.Dom).FadeOut().Ignore

            JQuery.Of("[id^=rightPlayer]").Text("[None]").Ignore
            JQuery.Of(rightPlayerHand.Dom).Children().Remove().Ignore
            JQuery.Of(rightPlayerdrawCardButton.Dom).FadeOut().Ignore

        let newGame gameGuid =
            clearGame()
            currentGameGuid := gameGuid
            JQuery.Of(gameGuidLabel.Dom).Text(gameGuid).Ignore

            leftPlayer := JavaScript.Undefined<Player>
            rightPlayer := JavaScript.Undefined<Player>

        let updateMana now max playerGuid =
            if playerGuid = (!leftPlayer).Guid then
                JQuery.Of("#proggressbarLunused")
                    .Css("width", (int(now) * 10).ToString() + "%").Ignore
                JQuery.Of("#proggressbarLused")
                    .Css("width", ((int(max) - int(now)) * 10).ToString() + "%").Ignore
            else
                JQuery.Of("#proggressbarRunused")
                    .Css("width", (int(now) * 10).ToString() + "%").Ignore
                JQuery.Of("#proggressbarRused")
                    .Css("width", ((int(max) - int(now)) * 10).ToString() + "%").Ignore

        let updateLeftPlayer (player) =
            leftPlayer := player
            JQuery.Of("#leftPlayerName").Text((!leftPlayer).Name).Ignore
            JQuery.Of("#leftPlayerClass").Text((!leftPlayer).HeroClass).Ignore
            JQuery.Of("#leftPlayerHeroPower").Text((fst (!leftPlayer).HeroPower).Name).Ignore
            match snd (!leftPlayer).HeroPower with
            | true -> JQuery.Of(leftPlayerUseHeroPowerButton.Dom).RemoveClass("btn-success").AddClass("btn-error").Attr("disabled", "true").Ignore
            | false -> JQuery.Of(leftPlayerUseHeroPowerButton.Dom).RemoveClass("btn-error").AddClass("btn-success").RemoveAttr("disabled").Ignore
            JQuery.Of("#leftPlayerRemainingCardsCount").Text(string <| List.length (!leftPlayer).Deck.CardIdList).Ignore
            JQuery.Of("#leftPlayerHealth").Text((!leftPlayer).HeroCharacter.Hp.ToString()).Ignore
            JQuery.Of(leftPlayerdrawCardButton.Dom).FadeIn().Ignore

            JQuery.Of(leftPlayerHand.Dom).Children().Remove().Ignore
            (!leftPlayer).Hand |> List.iter(fun item -> 
                match getCard item with
                | Success card ->
                    cardTemplateDiv card.Name card.Id |> leftPlayerHand.Append
                | Error msg -> ())
            updatePopover

        let updateRightPlayer (player) =
            rightPlayer := player
            JQuery.Of("#rightPlayerName").Text((!rightPlayer).Name).Ignore
            JQuery.Of("#rightPlayerClass").Text((!rightPlayer).HeroClass).Ignore
            JQuery.Of("#rightPlayerHeroPower").Text((fst (!rightPlayer).HeroPower).Name).Ignore
            JQuery.Of("#rightPlayerRemainingCardsCount").Text(string <| List.length (!rightPlayer).Deck.CardIdList).Ignore
            JQuery.Of("#rightPlayerHealth").Text((!rightPlayer).HeroCharacter.Hp.ToString()).Ignore
            JQuery.Of(rightPlayerdrawCardButton.Dom).FadeIn().Ignore


        let updatePlayer playerGuid =
            getPlayer playerGuid (!currentGameGuid)
                (fun res -> 
                    match res with
                    | Success player -> 
                        if (!leftPlayer) = JavaScript.Undefined<Player> || (!leftPlayer).Guid = player.Guid then
                            updateLeftPlayer player
                        else
                            updateRightPlayer player
                    | Error msg -> ())
        
        let updatePlayers () =
            [(!leftPlayer).Guid; (!rightPlayer).Guid] |> List.iter(fun e -> updatePlayer e)

        let playCard (cardId : string) (player : Player) =
            ()

        let addCardToHand (cardName : string) (cardId :string) (hand : Element) =
            cardTemplateDiv cardName cardId |> hand.Append
            updatePopover

        let setupButton =
            JQuery.Of(testButton.Dom)
                .Click(fun _ _ -> ()
                ).Ignore
            
            JQuery.Of(newGameButton.Dom)
                .Click(fun _ _ ->
                    NewGame (fun res ->
                        match res with
                        | Success gameGuid -> newGame gameGuid
                        | Error msg -> ())).Ignore

            JQuery.Of(registerPlayerButton.Dom)
                .Click(fun _ _ ->
                    let playerName =
                        if (!leftPlayer) = JavaScript.Undefined<Player> then
                            Some "LeftPlayer"
                        else if (!rightPlayer) = JavaScript.Undefined<Player> then
                            Some "RightPlayer"
                        else
                            None
                    match playerName with
                    | Some name ->
                        let classList = getPlayableClasses()
                        addItemsToAsk classList (fun selection -> 
                            registerPlayerWithClass name selection (!currentGameGuid) (fun res ->
                                match res with
                                | Success playerGuid -> updatePlayer playerGuid
                                | Error msg -> ())
                        )
                    | None -> ()).Ignore

            JQuery.Of(leftPlayerdrawCardButton.Dom)
                .Click(fun _ _ ->
                    drawCard (!leftPlayer).Guid (!currentGameGuid) (fun res ->
                        match res with
                        | Success card ->
                            addCardToHand card.Name card.Id leftPlayerHand
                            updatePlayer (!leftPlayer).Guid
                        | Error msg ->
                            ())).Hide().Ignore

            JQuery.Of(rightPlayerdrawCardButton.Dom)
                .Click(fun _ _ ->
                    drawCard (!rightPlayer).Guid (!currentGameGuid) (fun res ->
                        match res with
                        | Success card ->
                            addCardToHand card.Name card.Id rightPlayerHand
                            updatePlayer (!rightPlayer).Guid
                        | Error msg ->
                            ())).Hide().Ignore

            JQuery.Of(openModalButton.Dom).Hide().Ignore

            JQuery.Of(leftPlayerUseHeroPowerButton.Dom)
                .Click(fun _ _ ->
                    doesHeroPowerNeedTarget (fst (!leftPlayer).HeroPower).Name (fun needTarget ->
                        match needTarget with
                        | Success true ->
                            findTargetForHeroPower (!leftPlayer).Guid (!currentGameGuid) (fun res ->
                                match res with
                                | Success items ->
                                    let newItems =
                                        items |> List.map (fun item ->
                                            match getICharName (item) (!currentGameGuid) with
                                            | Success name -> name
                                            | Error msg -> "Fail to load name !"
                                        )
                                    let itemNameWithGuid = List.zip newItems items
                                    addItemsToAsk newItems (fun choice ->
                                        let selGuid = itemNameWithGuid |> List.find(fun (name, guid) -> name = choice) |> snd
                                        useHeroPower (!leftPlayer).Guid (Some selGuid) (!currentGameGuid) (fun afterUse ->
                                            match afterUse with
                                            | Success msg ->
                                                updatePlayers()
                                            | Error msg -> ()
                                        )
                                    )
                                | Error msg -> ()
                            )
                        | Success false ->
                            useHeroPower (!leftPlayer).Guid None (!currentGameGuid) (fun res ->
                                match res with
                                | Success msg ->
                                    updatePlayers()
                                | Error msg -> ()
                            )
                        | Error msg -> ())).Ignore

        Div [Attr.Class "col-md-12"] -< [
            Div [Attr.Class "page-header"] -< [H1 [Text "Game Simulation"]]

            Div [Attr.Class "panel panel-primary"] -< [
                Div [Attr.Class "panel-heading"] -- H3 [Attr.Class "panel-title"; Text "Game Control"]
                Div [Attr.Class "panel-body"] -< [
                    Div [Attr.Class "row clearfix"] -< [
                        Div [Attr.Class "col-xs-4"] -- newGameButton
                        Div [Attr.Class "col-xs-4"] -- registerPlayerButton
                        Div [Attr.Class "col-xs-4"] -- testButton
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
                            leftPlayerdrawCardButton
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
                            rightPlayerdrawCardButton
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
