namespace HearthstoneTextGame

open IntelliFactory.WebSharper
open IntelliFactory.WebSharper.Html
open IntelliFactory.WebSharper.JQuery

[<JavaScript>]
module Client =

    let doAsync doSth callback =
        async {
            let! result = doSth
            return callback result
        }
        |> Async.Start

    type Style =
        | Green
        | Default

    let btn text id (style : Style) = 
        let styleClass =
            match style with
            | Green -> "success"
            | Default -> "default"
        Button [ Id id
                 Attr.Type "button"
                 Attr.Class ("btn btn-block btn-" + styleClass)
                 Text text ]

    type GameClient () =
        member val Guid = JavaScript.Undefined<Guid> with get, set
        member val ActivePlayerGuid = JavaScript.Undefined<Guid> with get, set
        member val LeftPlayer = JavaScript.Undefined<Player> with get, set
        member val RightPlayer = JavaScript.Undefined<Player> with get, set
        member val NeedUpdate = false with get, set
        member __.Exist () = __.Guid <> JavaScript.Undefined<Guid>
        member __.HasLeftPlayer () = __.LeftPlayer <> JavaScript.Undefined<Player>
        member __.HasRightPlayer () = __.RightPlayer <> JavaScript.Undefined<Player>
        member __.Clear() =
            __.Guid <- JavaScript.Undefined<Guid>
            __.LeftPlayer <- JavaScript.Undefined<Player>
            __.RightPlayer <- JavaScript.Undefined<Player>

    [<Inline " notify($msg) ">]
    let notify (msg : string) = ()

    [<Inline " notifyError($msg) ">]
    let notifyError (msg : string) = ()

    [<Inline " notifySuccess($msg) ">]
    let notifySuccess (msg : string) = ()

    [<Inline " notifyImage($url) ">]
    let notifyImage (url : string) = ()

    [<Inline " showModal() ">]
    let showModal () = ()

    [<Inline " hideModal() ">]
    let hideModal () = ()

    [<Inline " startTimerBar() ">]
    let startTimerBar () = ()

    let currentGame = GameClient()
    let gameGuidLabel = Span [Text "[None]"]

    let newGameButton = btn "NewGame" "newGameBtn" Green
    let registerPlayerButton = btn "Register" "registerPlayerBtn" Default
    let startGameButton = btn "Start Game" "startGameBtn" Default
    let leftMulliganButton = btn "LeftMulligan" "mulliganBtn" Default
    let rightMulliganButton = btn "RightMulligan" "mulliganBtn" Default

    let leftPlayerEndTurnButton = btn "End Turn" "leftEndTurnBtn" Default
    let leftPlayerUseHeroPowerButton = btn "Use" "leftUseHeroPowerBtn" Default
    let leftPlayerFaceAttackButton = btn "Attack" "leftFaceAtkBtn" Default
    let leftPlayerHand = UL [Attr.Class "list-group"; Id "leftHand"; Rel "emptyChildren"]
    let leftPlayerBoard = UL [Attr.Class "list-group"; Id "leftBoard"; Rel "emptyChildren"]
    let leftPlayerManaBar = 
        Div [Attr.Class "progress"] -< [
            Div [ Attr.Class "progress-bar"
                  Attr.NewAttr "role" "progressbar"
                  Attr.Style "width: 0%;"
                  Id "leftProggressbarUnused" ]
            Div [ Attr.Class "progress-bar progress-bar-warning"
                  Attr.NewAttr "role" "progressbar"
                  Attr.Style "width: 0%;"
                  Id "leftProggressbarUsed" ]
        ]
    let leftPlayerInfoTable = 
        Table [Attr.Class "table table-hover myTable"; Id "leftInfo"] -< [
            THead [] -< [
                TR [] -< [
                    TH [Text "field"]
                    TH [Text "value"]
                ]
            ]
            TBody [] -< [
                TR [] -< [
                    TD [Text "Player Name"]
                    TD [Id "leftName"; Text "[None]"; Rel "clear"; ColSpan "2"]
                ]
                TR [] -< [
                    TD [Text "Player Class"]
                    TD [Id "leftClass"; Text "[None]"; Rel "clear"; ColSpan "2"]
                ]
                TR [] -< [
                    TD [Text "Hero Power"]
                    TD [Id "leftHeroPower"; Text "[None]"; Rel "clear"]
                    TD [] -- leftPlayerUseHeroPowerButton
                ]
                TR [] -< [
                    TD [Text "Remaining Cards"]
                    TD [Id "leftRemainingCardsCount"; Text "[None]"; Rel "clear"; ColSpan "2"]
                ]
                TR [] -< [
                    TD [Text "Health"]
                    TD [Id "leftHealth"; Text "[None]"; Rel "clear"; ColSpan "2"]
                ]
                TR [] -< [
                    TD [Text "Armour"]
                    TD [Id "leftArmour"; Text "[None]"; Rel "clear"; ColSpan "2"]
                ]
                TR [] -< [
                    TD [Text "Attack Value"]
                    TD [Id "leftAtkVal"; Text "[None]"; Rel "clear"]
                    TD [] -- leftPlayerFaceAttackButton
                ]
                TR [] -< [
                    TD [Text "Weapon"]
                    TD [Id "leftWeapon"; Text "[None]"; Rel "clear"; ColSpan "2"]
                ]
            ]
        ]

    let rightPlayerEndTurnButton = btn "End Turn" "rightEndTurnBtn" Default
    let rightPlayerUseHeroPowerButton = btn "Use" "rightUseHeroPowerBtn" Default
    let rightPlayerFaceAttackButton = btn "Attack" "rightFaceAtkBtn" Default
    let rightPlayerHand = UL [Attr.Class "list-group"; Id "rightHand"; Rel "emptyChildren"]
    let rightPlayerBoard = UL [Attr.Class "list-group"; Id "rightBoard"; Rel "emptyChildren"]
    let rightPlayerManaBar = 
        Div [Attr.Class "progress"] -< [
            Div [ Attr.Class "progress-bar"
                  Attr.NewAttr "role" "progressbar"
                  Attr.Style "width: 0%;"
                  Id "rightProggressbarUnused" ]
            Div [ Attr.Class "progress-bar progress-bar-warning"
                  Attr.NewAttr "role" "progressbar"
                  Attr.Style "width: 0%;"
                  Id "rightProggressbarUsed" ]
        ]
    let rightPlayerInfoTable = 
        Table [Attr.Class "table table-hover myTable"; Id "rightInfo"] -< [
            THead [] -< [
                TR [] -< [
                    TH [Text "field"]
                    TH [Text "value"]
                ]
            ]
            TBody [] -< [
                TR [] -< [
                    TD [Text "Player Name"]
                    TD [Id "rightName"; Text "[None]"; Rel "clear"; ColSpan "2"]
                ]
                TR [] -< [
                    TD [Text "Player Class"]
                    TD [Id "rightClass"; Text "[None]"; Rel "clear"; ColSpan "2"]
                ]
                TR [] -< [
                    TD [Text "Hero Power"]
                    TD [Id "rightHeroPower"; Text "[None]"; Rel "clear"]
                    TD [] -- rightPlayerUseHeroPowerButton
                ]
                TR [] -< [
                    TD [Text "Remaining Cards"]
                    TD [Id "rightRemainingCardsCount"; Text "[None]"; Rel "clear"; ColSpan "2"]
                ]
                TR [] -< [
                    TD [Text "Health"]
                    TD [Id "rightHealth"; Text "[None]"; Rel "clear"; ColSpan "2"]
                ]
                TR [] -< [
                    TD [Text "Armour"]
                    TD [Id "rightArmour"; Text "[None]"; Rel "clear"; ColSpan "2"]
                ]
                TR [] -< [
                    TD [Text "Attack Value"]
                    TD [Id "rightAtkVal"; Text "[None]"; Rel "clear"]
                    TD [] -- rightPlayerFaceAttackButton
                ]
                TR [] -< [
                    TD [Text "Weapon"]
                    TD [Id "rightWeapon"; Text "[None]"; Rel "clear"; ColSpan "2"]
                ]
            ]
        ]

    let askModalContentDiv = Div []

    let saveItemButton = 
        Button [Attr.Type "button"
                Attr.Class "btn btn-primary"
                Text "Save"]

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
                                Text "Close"] |>! OnClick (fun _ _ -> hideModal())
                        saveItemButton
                    ]
                ]
            ]
        ]
    let modalOpened = ref false;
    let initElements () =
        JQuery.Of(modalDiv.Dom).On("shown.bs.modal", (fun _ -> modalOpened := true; true))
        JQuery.Of(modalDiv.Dom).On("hidden.bs.modal", (fun _ -> modalOpened := false; true))

    let openModal () =
        if (!modalOpened) then
            JQuery.Of(modalDiv.Dom).Off("hidden.bs.modal")
            JQuery.Of(modalDiv.Dom).On("hidden.bs.modal", (fun _ -> 
                showModal()
                JQuery.Of(modalDiv.Dom).Off("hidden.bs.modal")
                JQuery.Of(modalDiv.Dom).On("hidden.bs.modal", (fun _ -> modalOpened := false; true))
                true))
        else
            showModal()

    let addItemsToAsk (items : ('a * string) list) (callback : 'a -> unit) =
        let askItems = Div [Attr.Class "list-group"]
        items |> List.iter(fun (itemGuid, itemName) ->
            let newItem = A [Attr.Class "list-group-item"; Text itemName; HRef "#"]          
            JQuery.Of(newItem.Dom).Click(fun elem evt ->
                evt.PreventDefault()
                JQuery.Of(askItems.Dom).Children("a").RemoveClass("active").Ignore             
                JQuery.Of(elem).AddClass("active").Ignore
                JQuery.Of(saveItemButton.Dom)
                    .Unbind("click")
                    .Click(fun _ _ -> hideModal(); callback itemGuid).Ignore
                ).Ignore
            askItems.Append(newItem)
            )
        JQuery.Of(askModalContentDiv.Dom).Empty().Ignore
        JQuery.Of(askModalContentDiv.Dom).Append(JQuery.Of(askItems.Dom)).Ignore
        JQuery.Of(saveItemButton.Dom).Unbind("click").Click(fun _ _ -> hideModal()).Ignore
        openModal()

    let addItemsToAskMultiple (items : ('a * string) list) (callback : 'a list -> unit) =
        let (sels : 'a list ref) = ref []
        let askItems = Div [Attr.Class "list-group"]
        items |> List.iter(fun (itemGuid, itemName) ->
            let newItem = A [Attr.Class "list-group-item"; Text itemName; HRef "#"]          
            JQuery.Of(newItem.Dom).Click(fun elem evt ->
                evt.PreventDefault()
                if JQuery.Of(elem).HasClass("active") then
                    sels := (!sels) |> List.filter(fun e -> e <> itemGuid)
                    JQuery.Of(elem).RemoveClass("active").Ignore
                else           
                    sels := itemGuid :: !sels
                    JQuery.Of(elem).AddClass("active").Ignore
                ).Ignore
            askItems.Append(newItem)
            )
        JQuery.Of(askModalContentDiv.Dom).Empty().Ignore
        JQuery.Of(askModalContentDiv.Dom).Append(JQuery.Of(askItems.Dom)).Ignore
        JQuery.Of(saveItemButton.Dom).Unbind("click").Click(fun _ _ -> hideModal(); callback (!sels)).Ignore
        openModal()

    let addItemsToAskForPosition (items : string list) (callback : int -> unit) =
        let askItems = Div [Attr.Class "list-group"]
        for (idx, item) in items |> List.mapi(fun i it -> i, it) do
            let pos = A [Attr.Class "list-group-item"; Text "->"; HRef "#"]
            JQuery.Of(pos.Dom).Click(fun elem evt ->
                evt.PreventDefault()
                JQuery.Of(askItems.Dom).Children("a").RemoveClass("active").Ignore
                JQuery.Of(elem).AddClass("active").Ignore
                JQuery.Of(saveItemButton.Dom)
                    .Unbind("click")
                    .Click(fun _ _ -> hideModal(); callback idx)
                    .Ignore
            ).Ignore
            askItems.Append(pos)
            let newItem = A [Attr.Class "list-group-item"; Text item]
            askItems.Append(newItem)

        // Add item for last position
        let pos = A [Attr.Class "list-group-item"; Text "->"; HRef "#"]
        JQuery.Of(pos.Dom).Click(fun elem evt ->
            evt.PreventDefault()
            JQuery.Of(askItems.Dom).Children("a").RemoveClass("active").Ignore
            JQuery.Of(elem).AddClass("active").Ignore
            JQuery.Of(saveItemButton.Dom)
                .Unbind("click")
                .Click(fun _ _ -> hideModal(); callback items.Length)
                .Ignore
        ).Ignore
        askItems.Append(pos)
        JQuery.Of(askModalContentDiv.Dom).Empty().Ignore
        JQuery.Of(askModalContentDiv.Dom).Append(JQuery.Of(askItems.Dom)).Ignore
        JQuery.Of(saveItemButton.Dom).Unbind("click").Click(fun _ _ -> hideModal()).Ignore
        openModal()

    let cardTemplateDiv (playable : bool) (card : CardOnHand) (owner : Player) =

        let cardImgUrl = "http://wow.zamimg.com/images/hearthstone/cards/enus/medium/" + card.Card.Id + ".png"
        let previewButton =
            JQuery.Of("<button />")
                .AddClass("btn btn-default")
                .Attr("type", "button")
                .Text("Image")
                .Click(fun _ _ ->
                    notifyImage(cardImgUrl)
                )
                
        let playCardButton =
            let item =
                JQuery.Of("<button />")
                    .Attr("type", "button")
                    .Text("Play")
                    .Click(fun _ _ ->
                        if not playable then notifyError("Cannot play card")
                        else
                            doAsync (Remoting.DoesCardNeedTarget card.Card.Id)
                                (fun needTarget ->
                                    match needTarget with
                                    | false ->
                                        match card.Card.Type with
                                        | "Minion" ->
                                            addItemsToAskForPosition (owner.Minions |> List.map(fun e -> e.Card.Name))
                                                (fun idx ->
                                                    doAsync (Remoting.PlayCard card (Some idx) None owner.Guid currentGame.Guid)
                                                        (fun ret ->
                                                            match ret with
                                                            | Success msg -> notifySuccess(msg)
                                                            | Error msg -> notifyError(msg)
                                                        )
                                                )
                                        | _ ->
                                            doAsync (Remoting.PlayCard card None None owner.Guid currentGame.Guid)
                                                (fun ret ->
                                                    match ret with
                                                    | Success msg -> notifySuccess(msg)
                                                    | Error msg -> notifyError(msg)
                                                )        
                                    | true ->
                                        doAsync (Remoting.FindTargetForCard card.Card owner.Guid currentGame.Guid)
                                            (fun ret ->
                                                match ret with
                                                | Error msg -> notifyError(msg)
                                                | Success items ->
                                                    let itemName= 
                                                        items |> List.map (fun e ->
                                                            match Remoting.GetICharName e currentGame.Guid with
                                                            | Success name -> name
                                                            | Error msg -> "Fail to load name !"
                                                        )
                                                    let itemNameWithGuid = List.zip items itemName
                                                    match card.Card.Type with
                                                    | "Minion" ->
                                                        addItemsToAskForPosition (owner.Minions |> List.map(fun e -> e.Card.Name))
                                                            (fun idx ->
                                                                addItemsToAsk itemNameWithGuid
                                                                    (fun sel ->
                                                                        doAsync (Remoting.PlayCard card (Some idx) (Some sel) owner.Guid currentGame.Guid)
                                                                            (fun res ->
                                                                                match res with
                                                                                | Success msg -> notifySuccess(msg)
                                                                                | Error msg -> notifyError(msg)
                                                                            )
                                                                    )
                                                            
                                                            )
                                                    | _ ->
                                                        addItemsToAsk itemNameWithGuid
                                                            (fun sel ->
                                                                doAsync (Remoting.PlayCard card None (Some sel) owner.Guid currentGame.Guid)
                                                                    (fun res ->
                                                                        match res with
                                                                        | Success msg -> notifySuccess(msg)
                                                                        | Error msg -> notifyError(msg)
                                                                    )
                                                            )
                                            )
                                )
                    )
            if playable then item.AddClass("btn btn-success")
            else item.AddClass("btn btn-default").Attr("disabled", "true")
        
        let cost = 
            let costLabel = 
                JQuery.Of("<button />")
                    .Attr("role", "button")
                    .Text(card.Cost.ToString())
            if card.Cost > card.Card.Cost.Value then
                costLabel.AddClass("btn btn-danger")
            else if card.Cost < card.Card.Cost.Value then
                costLabel.AddClass("btn btn-success")
            else
                costLabel.AddClass("btn btn-primary")

        let buttonGroup = 
            let div = Div [Attr.Class "btn-group"]
            JQuery.Of(div.Dom).Append(previewButton).Append(playCardButton)

        let cardInfo =
            let div = Div [Attr.Class "btn-group"]
            

            let atk =
                if card.Card.Type = "Minion" || card.Card.Type = "Weapon" then
                    JQuery.Of("<button />").Attr("role", "button").AddClass("btn btn-default").Text(if card.Card.Attack.IsSome then card.Card.Attack.Value.ToString() else "")
                else
                    JQuery.Of("")
            let hpOrDu =
                if card.Card.Type = "Minion" then
                    JQuery.Of("<button />").Attr("role", "button").AddClass("btn btn-default").Text(if card.Card.Health.IsSome then card.Card.Health.Value.ToString() else "")
                else if card.Card.Type = "Weapon" then
                    JQuery.Of("<button />").Attr("role", "button").AddClass("btn btn-default").Text(if card.Card.Durability.IsSome then card.Card.Durability.Value.ToString() else "")
                else
                    JQuery.Of("")

            JQuery.Of(div.Dom).Append(atk).Append(hpOrDu)

        let name = 
            let nameLabel = JQuery.Of("<h5 />").Text(card.Card.Name).AddClass("pull-right")
            match card.Card.Rarity with
            | Some "Legendary" -> nameLabel.Css("color", "orange")
            | Some "Epic" -> nameLabel.Css("color", "violet")
            | Some "Rare" -> nameLabel.Css("color", "blue")
            | Some "Common" -> nameLabel.Css("color", "black")
            | _ -> nameLabel.Css("color", "gray")

        let newItem = LI []
        JQuery.Of(newItem.Dom)
            .AddClass("list-group-item")
            .Append(JQuery.Of("<div />").AddClass("row").AddClass("clearfix")
                .Append(JQuery.Of("<div />").AddClass("col-xs-4").Append(buttonGroup))
                .Append(JQuery.Of("<div />").AddClass("col-xs-1").Append(cost))
                .Append(JQuery.Of("<div />").AddClass("col-xs-3").Append(cardInfo))
                .Append(JQuery.Of("<div />").AddClass("col-xs-4").Append(name))
            ).Ignore
        newItem

    let doAttack (source : Guid) (owner : Player) =
        doAsync (Remoting.FindTargetToAttack owner.Guid currentGame.Guid)
            (fun res ->
                match res with
                | Success items ->
                    let itemNames =
                        items |> List.map (fun item ->
                            match Remoting.GetICharName (item) currentGame.Guid with
                            | Success name -> name
                            | Error msg -> "Fail to load name !"
                        )
                    let itemNameWithGuid = List.zip items itemNames 
                    addItemsToAsk itemNameWithGuid (fun choice ->
                        doAsync (Remoting.AttackIChar source choice currentGame.Guid)
                            (fun afterUse ->
                                match afterUse with
                                | Success msg -> notifySuccess(msg)
                                | Error msg -> notifyError(msg)
                            )
                    )
                | Error msg -> notifyError(msg)
            )

    let minionTemplateDiv (attackable : bool) (minion : Minion) (owner : Player) =
        let cardImgUrl = "http://wow.zamimg.com/images/hearthstone/cards/enus/medium/" + minion.Card.Id + ".png"
        
        let previewButton =
            JQuery.Of("<button />")
                .AddClass("btn btn-default")
                .Attr("type", "button")
                .Text("Image")
                .Click(fun _ _ ->
                    notifyImage(cardImgUrl)
                )
        let attackButton =
            let item =
                JQuery.Of("<button />")
                    .Attr("type", "button")
                    .Text("Attack")
                    .Click(fun _ _ ->
                        doAttack minion.Guid owner
                    )
            match attackable with
            | true -> item.AddClass("btn btn-success")
            | false -> item.AddClass("btn btn-default").Attr("disabled", "true")
        let buttonGroup = 
            let div = Div [Attr.Class "btn-group"]
            JQuery.Of(div.Dom).Append(previewButton).Append(attackButton)

        let atk = 
            let elem = 
                JQuery.Of("<button />")
                    .Text(minion.AttackValue.ToString())
                    .Attr("role", "button")
            if minion.AttackValue > minion.Card.Attack.Value then
                elem.AddClass("btn btn-success")
            else
                elem.AddClass("btn btn-default")
        
        let hp =
            let elem = 
                JQuery.Of("<button />")
                    .Text(minion.CurrentHealth.ToString())
            if minion.CurrentHealth > minion.Card.Health.Value then
                elem.AddClass("btn btn-success")
            else if minion.CurrentHealth < minion.MaxHealth then
                elem.AddClass("btn btn-danger")
            else
                elem.AddClass("btn btn-default")

        let props =
            let mutable prop = []
            let label (text) =
                JQuery.Of("<li />").Append(JQuery.Of("<a />").Text(text))
            if minion.HasDivineShield then prop <- label ("DvnShld") :: prop
            if minion.HasTaunt then prop <- label ("Tnt") :: prop

            let div = JQuery.Of("<div />").AddClass("btn-group")
            let div2 = JQuery.Of("<button />").AddClass("btn btn-default dropdown-toggle").Attr("data-toggle", "dropdown").Text("Status").Append(JQuery.Of("<span class='caret'></span>"))
            let div3 = JQuery.Of("<ul />").AddClass("dropdown-menu").Attr("role", "menu")
            if prop.Length <> 0 then
                prop |> List.iter(fun e -> div3.Append(e).Ignore)
            else
                div2.Attr("disabled", "disabled").Ignore
            div.Append(div2).Append(div3)

        let buttonGroup2 = 
            let div = Div [Attr.Class "btn-group"]
            JQuery.Of(div.Dom).Append(atk).Append(hp).Append(props)

        let nameLabel = JQuery.Of("<h5 />").Text(minion.Card.Name).AddClass("pull-right")
        match minion.Card.Rarity with
        | Some "Legendary" -> nameLabel.Css("color", "orange").Ignore
        | Some "Epic" -> nameLabel.Css("color", "violet").Ignore
        | Some "Rare" -> nameLabel.Css("color", "blue").Ignore
        | Some "Common" -> nameLabel.Css("color", "black").Ignore
        | _ -> nameLabel.Css("color", "gray").Ignore


        let newItem = LI []
        JQuery.Of(newItem.Dom)
            .AddClass("list-group-item")
            .Append(JQuery.Of("<div />").AddClass("row").AddClass("clearfix")
                .Append(JQuery.Of("<div />").AddClass("col-xs-4").Append(buttonGroup))
                .Append(JQuery.Of("<div />").AddClass("col-xs-4").Append(buttonGroup2))
                .Append(JQuery.Of("<div />").AddClass("col-xs-4").Append(nameLabel))
            ).Ignore
        newItem

    let updateMana now max playerGuid =
        let player =
            if playerGuid = currentGame.LeftPlayer.Guid then "left"
            else "right"
        JQuery.Of("#" + player + "ProggressbarUnused")
            .Text(now.ToString())
            .Css("width", (int(now) * 10).ToString() + "%").Ignore
        JQuery.Of("#" + player + "ProggressbarUsed")
            .Css("width", ((int(max) - int(now)) * 10).ToString() + "%").Ignore

    let updatePlayer (player : Player) =
        let playerStr = 
            if not <| currentGame.HasLeftPlayer() || currentGame.LeftPlayer.Guid = player.Guid then
                currentGame.LeftPlayer <- player
                "left"
            else if not <| currentGame.HasRightPlayer() || currentGame.RightPlayer.Guid = player.Guid then
                currentGame.RightPlayer <- player
                "right"
            else
                ""
        if playerStr = "" then JavaScript.Log("Unable to updatePlayer: " + player.Guid.value)
        else
            JQuery.Of("#" + playerStr + "Panel").Show().Ignore
            let isActive = currentGame.ActivePlayerGuid = player.Guid
            match isActive with
            | true ->
                JQuery.Of("#" + playerStr + "Panel").AddClass("panel-success").Ignore
                JQuery.Of("#" + playerStr + "EndTurnBtn")
//                    .RemoveClass("btn-success")
                    .AddClass("btn-warning")
                    .RemoveAttr("disabled").Ignore
                JQuery.Of("#" + playerStr + "ProggressbarUnused").AddClass("progress-bar-striped active").Ignore
            | false ->
                JQuery.Of("#" + playerStr + "Panel").RemoveClass("panel-success").Ignore
                JQuery.Of("#" + playerStr + "EndTurnBtn")
//                    .RemoveClass("btn-success")
                    .RemoveClass("btn-warning")
                    .Attr("disabled", "true").Ignore
                JQuery.Of("#" + playerStr + "ProggressbarUnused").RemoveClass("progress-bar-striped active").Ignore

            JQuery.Of("#" + playerStr + "Name").Text(player.Name).Ignore
            JQuery.Of("#" + playerStr + "Class").Text(player.HeroClass).Ignore
            JQuery.Of("#" + playerStr + "HeroPower").Text(player.HeroPower.Name).Ignore
            JQuery.Of("#" + playerStr + "RemainingCardsCount").Text(string player.Deck.CardIdList.Length).Ignore
            JQuery.Of("#" + playerStr + "Health").Text(player.Face.Hp.ToString()).Ignore
            match player.Face.Weapon with
            | Some weapon ->
                JQuery.Of("#" + playerStr + "Weapon").Text("").Ignore
                JQuery.Of("<div />").AddClass("row")
                    .Append(JQuery.Of("<div />").AddClass("col-xs-9").Text(weapon.Card.Name))
                    .Append(JQuery.Of("<div />").AddClass("col-xs-1").AddClass("label label-default").Text(weapon.Attack.ToString()))
                    .Append(JQuery.Of("<div />").AddClass("col-xs-1").AddClass("label label-default").Text(weapon.Durability.ToString()))
                    .AppendTo(JQuery.Of("#" + playerStr + "Weapon")).Ignore
                JQuery.Of("#" + playerStr + "Weapon").Parent().FadeIn().Ignore
            | None ->
                JQuery.Of("#" + playerStr + "Weapon").Parent().FadeOut().Ignore

            match player.Face.Armour with
            | GreaterThanZero ->
                JQuery.Of("#" + playerStr + "Armour").Text(player.Face.Armour.ToString()).Parent().FadeIn().Ignore
            | _ ->
                JQuery.Of("#" + playerStr + "Armour").Parent().FadeOut().Ignore
            
            let atkValue =
                if player.Face.WeaponActivated && player.Face.Weapon.IsSome then player.Face.AttackValue + player.Face.Weapon.Value.Attack
                else player.Face.AttackValue

            match atkValue with
            | EqualZero ->
                JQuery.Of("#" + playerStr + "AtkVal").Parent().FadeOut().Ignore
            | _ ->
                JQuery.Of("#" + playerStr + "AtkVal").Text(atkValue.ToString()).Parent().FadeIn().Ignore
                JQuery.Of("#" + playerStr + "FaceAtkBtn").Show().Ignore
                if player.Face.AttackCount < player.Face.AttackTokens then
                    JQuery.Of("#" + playerStr + "FaceAtkBtn").AddClass("btn-success").RemoveAttr("disabled").Ignore
                else
                    JQuery.Of("#" + playerStr + "FaceAtkBtn").RemoveClass("btn-success").Attr("disabled", "true").Ignore
            JQuery.Of("#" + playerStr + "UseHeroPowerBtn").FadeIn().Ignore
            match (not player.HeroPowerUsed) && isActive && (player.HeroPower.Cost <= player.CurrentMana) with
            | true -> JQuery.Of("#" + playerStr + "UseHeroPowerBtn").AddClass("btn-success").RemoveAttr("disabled").Ignore
            | false -> JQuery.Of("#" + playerStr + "UseHeroPowerBtn").RemoveClass("btn-success").Attr("disabled", "true").Ignore
            
            updateMana player.CurrentMana player.MaxMana player.Guid

            JQuery.Of("#" + playerStr + "Hand").Children().Remove().Ignore
            player.Hand |> List.iter(fun card ->
                let newCard = cardTemplateDiv ((card.Cost <= player.CurrentMana) && isActive) card player
                JQuery.Of("#" + playerStr + "Hand").Append(JQuery.Of(newCard.Dom)).Ignore
            )

            JQuery.Of("#" + playerStr + "Board").Children().Remove().Ignore
            player.Minions |> List.iter(fun minion ->
                let minion = minionTemplateDiv ((minion.AttackCount < minion.AttackTokens) && isActive && minion.AttackValue > 0) minion player
                JQuery.Of("#" + playerStr + "Board").Append(JQuery.Of(minion.Dom)).Ignore
            )

//    let clearGame () =
//        currentGame.Clear()
//        JQuery.Of("[rel='emptyChildren']").Children().Remove().Ignore
//        [ "left"; "right" ]
//        |> List.iter(fun playerStr -> JQuery.Of("#" + playerStr + "Panel").Hide().Ignore)

    let rec updatePlayers () =
        try
            if currentGame.Exist() then
                doAsync (Remoting.AskForUpdate(currentGame.Guid))
                    (fun (gameGuid, cont) ->
                        if cont && (currentGame.Guid = gameGuid) then updatePlayers ()
                        doAsync (Remoting.GetActivePlayerGuid(currentGame.Guid))
                            (fun ret -> 
                                match ret with
                                | Success guid -> 
                                    currentGame.ActivePlayerGuid <- guid
                                    [ if currentGame.HasLeftPlayer() then yield currentGame.LeftPlayer.Guid
                                      if currentGame.HasRightPlayer() then yield currentGame.RightPlayer.Guid ]
                                    |> List.iter(fun playerGuid ->
                                        doAsync (Remoting.GetPlayer playerGuid currentGame.Guid)
                                            (fun res -> 
                                                match res with
                                                | Success player -> updatePlayer player
                                                | Error msg -> notifyError(msg))
                                        )
                                | Error msg -> notifyError(msg)
                            )
                    )
        with
        | e -> 
            JavaScript.Log("Error: " + e.ToString())
            updatePlayers()

    let newGame () =
        initElements()
        if currentGame.Exist() then notify("Refresh page to start new game !")
        else
            doAsync (Remoting.NewGame())
                (fun res ->
                    match res with
                    | Success gameGuid ->
                        currentGame.Guid <- gameGuid
                        JQuery.Of(gameGuidLabel.Dom).Text(gameGuid.value).Ignore
                        JQuery.Of(newGameButton.Dom).RemoveClass("btn-success").AddClass("btn-warning").Ignore
                        updatePlayers ()

                        
                    | Error msg -> notifyError(msg))
        

    let registerPlayer () =
        if currentGame.HasLeftPlayer() && currentGame.HasRightPlayer() then
            notifyError("Max number of players registered")
        else
            let classList = Remoting.GetPlayableClass()
            let classWithIdList = List.zip classList classList
            addItemsToAsk classWithIdList (fun selection ->
                doAsync (Remoting.GetPredefinedDeck())
                    (fun decks ->
                        let classDeck = decks |> List.filter(fun e -> e.DeckClass = selection)
                        let deckWithName = (List.zip classDeck (classDeck |> List.map(fun e -> e.Name))) @ [ JavaScript.Undefined<Deck>, "Random" ]
                        addItemsToAsk deckWithName
                            (fun selDeck ->
                                if selDeck = JavaScript.Undefined<Deck> then
                                    doAsync (Remoting.RegisterPlayerWithClass (selection + " Player") selection currentGame.Guid) 
                                        (fun res ->
                                            match res with
                                            | Success playerGuid -> 
                                                notifySuccess("Registered " + playerGuid.value)
                                                doAsync (Remoting.GetPlayer playerGuid currentGame.Guid)
                                                    (fun ret ->
                                                        match ret with
                                                        | Success player -> updatePlayer player
                                                        | Error msg -> notifyError(msg))
                                            | Error msg -> notifyError(msg)
                                        )
                                else
                                    doAsync (Remoting.RegisterPlayerWithDeck selDeck.Name selDeck currentGame.Guid)
                                        (fun res ->
                                            match res with
                                            | Success playerGuid -> 
                                                notifySuccess("Registered " + playerGuid.value)
                                                doAsync (Remoting.GetPlayer playerGuid currentGame.Guid)
                                                    (fun ret ->
                                                        match ret with
                                                        | Success player -> updatePlayer player
                                                        | Error msg -> notifyError(msg))
                                            | Error msg -> notifyError(msg)
                                        )
                            )
                    )                      
            )

    let useHeroPower (player : Player) =
        doAsync (Remoting.DoesHeroPowerNeedTarget (player.HeroPower.Name))
            (fun needTarget ->
                match needTarget with
                | Success true ->
                    doAsync (Remoting.FindTargetForHeroPower player.Guid currentGame.Guid)
                        (fun res ->
                            match res with
                            | Success items ->
                                let itemNames =
                                    items |> List.map (fun item ->
                                        match Remoting.GetICharName (item) currentGame.Guid with
                                        | Success name -> name
                                        | Error msg -> "Fail to load name !"
                                    )
                                let itemNameWithGuid = List.zip items itemNames 
                                addItemsToAsk itemNameWithGuid (fun choice ->
                                    doAsync (Remoting.UseHeroPower player.Guid (Some choice) currentGame.Guid)
                                        (fun afterUse ->
                                            match afterUse with
                                            | Success msg -> notifySuccess(msg)
                                            | Error msg -> notifyError(msg)
                                        )
                                )
                            | Error msg -> ()
                        )
                | Success false ->
                    doAsync (Remoting.UseHeroPower player.Guid None currentGame.Guid)
                        (fun res ->
                            match res with
                            | Success msg -> notifySuccess(msg)
                            | Error msg -> notifyError(msg)
                        )
                | Error msg -> notifyError(msg))

    let doMulligan (player : Player) =
        if currentGame.Exist() then
            doAsync (Remoting.GetMulligan player.Guid currentGame.Guid)
                (fun ret ->
                    match ret with
                    | Success stringIdList ->
                        let itemNames =
                            stringIdList |> List.map(fun e -> 
                                match Remoting.GetCard e with
                                | Success card -> card.Name
                                | Error msg -> "Cannot load name !"
                                )
                        let itemNamesWithGuid = List.zip stringIdList itemNames
                        addItemsToAskMultiple itemNamesWithGuid (fun choiceList ->
                            doAsync (Remoting.ReturnMulligan choiceList player.Guid currentGame.Guid)
                                (fun res ->
                                    match res with
                                    | Success finished ->
                                        if finished then
                                            notifySuccess("Mulligan ended. Game starting ..")
                                        else
                                            notify("Wait for other players to mulligan")
                                    | Error msg -> notifyError(msg)
                                )
                        )
                    | Error msg -> notifyError(msg)
                )
        else
            notify("Start game first")

    let setupButton =

        JQuery.Of(leftMulliganButton.Dom)
            .Click(fun _ _ ->
                doMulligan(currentGame.LeftPlayer)
            ).Ignore

        JQuery.Of(rightMulliganButton.Dom)
            .Click(fun _ _ ->
                doMulligan(currentGame.RightPlayer)
            ).Ignore

        JQuery.Of(startGameButton.Dom)
            .Click(fun _ _ ->
                if currentGame.Exist() then
                    doAsync (Remoting.StartGame currentGame.Guid)
                        (fun ret ->
                            match ret with
                            | Success msg -> notify("Mulligan started")
                            | Error msg -> notifyError(msg)
                        )
            ).Ignore
            
        JQuery.Of(newGameButton.Dom)
            .Click(fun _ _ ->
                newGame()
            ).Ignore

        JQuery.Of(registerPlayerButton.Dom)
            .Click(fun _ _ ->
                registerPlayer()
            ).Ignore

        JQuery.Of(leftPlayerEndTurnButton.Dom)
            .Click(fun _ _ ->
                startTimerBar()
                doAsync (Remoting.EndTurn currentGame.LeftPlayer.Guid currentGame.Guid)
                    (fun ret ->
                        match ret with
                        | Success msg -> notifySuccess(msg)
                        | Error msg -> notifyError(msg)
                    )
                )
            .Attr("disabled", "true")
            .Ignore

        JQuery.Of(rightPlayerEndTurnButton.Dom)
            .Click(fun _ _ ->
                startTimerBar()
                doAsync (Remoting.EndTurn currentGame.RightPlayer.Guid currentGame.Guid)
                    (fun ret ->
                        match ret with
                        | Success msg -> notifySuccess(msg)
                        | Error msg -> notifyError(msg)
                    )
                )
            .Attr("disabled", "true")
            .Ignore           
            
        JQuery.Of(leftPlayerUseHeroPowerButton.Dom)
            .Click(fun _ _ -> useHeroPower currentGame.LeftPlayer).Hide().Ignore

        JQuery.Of(rightPlayerUseHeroPowerButton.Dom)
            .Click(fun _ _ -> useHeroPower currentGame.RightPlayer).Hide().Ignore

        JQuery.Of(leftPlayerFaceAttackButton.Dom)
            .Click(fun _ _ -> doAttack currentGame.LeftPlayer.Face.Guid currentGame.LeftPlayer).Hide().Ignore

        JQuery.Of(rightPlayerFaceAttackButton.Dom)
            .Click(fun _ _ -> doAttack currentGame.RightPlayer.Face.Guid currentGame.RightPlayer).Hide().Ignore
    

    let Main () =

        Div [Attr.Class "col-md-12"] -< [
            Div [Attr.Class "panel panel-primary"] -< [
                Div [Attr.Class "panel-heading"] -- H3 [Attr.Class "panel-title"; Text "Game Control"]
                Div [Attr.Class "panel-body"] -< [
                    Div [Attr.Class "row clearfix"] -< [
                        Div [Attr.Class "col-sm-4"] -- newGameButton
                        Div [Attr.Class "col-sm-4"] -- registerPlayerButton
                        Div [Attr.Class "col-sm-4"] -- startGameButton
                    ]
                    Div [Attr.Class "row clearfix"] -< [
                        Div [Attr.Class "col-sm-6"] -- leftMulliganButton
                        Div [Attr.Class "col-sm-6"] -- rightMulliganButton
                    ]
                ]
                Div [Attr.Class "panel-footer"] -< [
                    Div [Attr.Class "row clearfix"] -< [
                        H4 [Attr.Class "col-sm-2"] -< [
                            Span [Attr.Class "label label-info center-block"; Text "Game Guid"]
                            |>! OnClick(fun _ _ -> updatePlayers())
                        ]
                        H4 [Attr.Class "col-sm-10 text-center"] -- gameGuidLabel
                    ]
                ]
            ]

            Div [Attr.Class "row clearfix"] -< [
                Div [Attr.Class "col-md-6 column"] -< [
                    Div [Attr.Class "panel panel-default"; Id "leftPanel"] -< [
                        Div [Attr.Class "panel-heading"] -- H3 [Attr.Class "panel-title"; Text "Left Player"]
                        Div [Attr.Class "panel-body"] -< [ 
                            leftPlayerEndTurnButton
                            leftPlayerInfoTable
                            leftPlayerManaBar
                            leftPlayerHand
                        ]
                    ]
                ]
                Div [Attr.Class "col-md-6 column"] -< [
                    Div [Attr.Class "panel panel-default"; Id "rightPanel"] -< [
                        Div [Attr.Class "panel-heading"] -- H3 [Attr.Class "panel-title"; Text "Right Player"]
                        Div [Attr.Class "panel-body"] -< [ 
                            rightPlayerEndTurnButton
                            rightPlayerInfoTable
                            rightPlayerManaBar
                            rightPlayerHand
                        ]
                    ]
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
            Div [Id "timerBar"]
        ]

    let About () =
        Div [Attr.Class "jumbotron"] -< [
            H3 [Text "@Tue AKIO 2014"]
        ]
