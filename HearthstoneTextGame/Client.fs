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
        member val LastChanged = JavaScript.Undefined<int64> with get, set
        member val ActivePlayerGuid = JavaScript.Undefined<Guid> with get, set
        member val LeftPlayer = JavaScript.Undefined<Player> with get, set
        member val RightPlayer = JavaScript.Undefined<Player> with get, set
        member val NeedUpdate = false with get, set
        member __.Exist () = __.Guid <> JavaScript.Undefined<Guid>
        member __.HasLeftPlayer () = __.LeftPlayer <> JavaScript.Undefined<Player>
        member __.HasRightPlayer () = __.RightPlayer <> JavaScript.Undefined<Player>
        member __.Clear() =
            __.Guid <- JavaScript.Undefined<Guid>
            __.LastChanged <- JavaScript.Undefined<int64> 
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

    let currentGame = GameClient()
    let gameGuidLabel = Span [Text "[None]"]

    let newGameButton = btn "NewGame" "newGameBtn" Green
    let registerPlayerButton = btn "Register" "registerPlayerBtn" Default
    let startGameButton = btn "Start Game" "startGameBtn" Default

    let leftPlayerEndTurnButton = btn "End Turn" "leftEndTurnBtn" Default
    let leftPlayerUseHeroPowerButton = btn "Use Hero Power" "leftUseHeroPowerBtn" Default
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
        Table [Attr.Class "table table-hover"; Id "leftInfo"] -< [
            THead [] -< [
                TR [] -< [
                    TH [Text "field"]
                    TH [Text "value"]
                ]
            ]
            TBody [] -< [
                TR [] -< [
                    TD [Text "Player Name"]
                    TD [Id "leftName"; Text "[None]"; Rel "clear"]
                ]
                TR [] -< [
                    TD [Text "Player Class"]
                    TD [Id "leftClass"; Text "[None]"; Rel "clear"]
                ]
                TR [] -< [
                    TD [Text "Hero Power"]
                    TD [Id "leftHeroPower"; Text "[None]"; Rel "clear"]
                ]
                TR [] -< [
                    TD [Text "Remaining Cards"]
                    TD [Id "leftRemainingCardsCount"; Text "[None]"; Rel "clear"]
                ]
                TR [] -< [
                    TD [Text "Health"]
                    TD [Id "leftHealth"; Text "[None]"; Rel "clear"]
                ]
                TR [] -< [
                    TD [Text "Armour"]
                    TD [Id "leftArmour"; Text "[None]"; Rel "clear"]
                ]
                TR [] -< [
                    TD [Text "Attack Value"]
                    TD [Id "leftAtkVal"; Text "[None]"; Rel "clear"]
                ]
                TR [] -< [
                    TD [Text "Weapon"]
                    TD [Id "leftWeapon"; Text "[None]"; Rel "clear"]
                ]
            ]
        ]

    let rightPlayerEndTurnButton = btn "End Turn" "rightEndTurnBtn" Default
    let rightPlayerUseHeroPowerButton = btn "Use Hero Power" "rightUseHeroPowerBtn" Default
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
        Table [Attr.Class "table table-hover"; Id "rightInfo"] -< [
            THead [] -< [
                TR [] -< [
                    TH [Text "field"]
                    TH [Text "value"]
                ]
            ]
            TBody [] -< [
                TR [] -< [
                    TD [Text "Player Name"]
                    TD [Id "rightName"; Text "[None]"; Rel "clear"]
                ]
                TR [] -< [
                    TD [Text "Player Class"]
                    TD [Id "rightClass"; Text "[None]"; Rel "clear"]
                ]
                TR [] -< [
                    TD [Text "Hero Power"]
                    TD [Id "rightHeroPower"; Text "[None]"; Rel "clear"]
                ]
                TR [] -< [
                    TD [Text "Remaining Cards"]
                    TD [Id "rightRemainingCardsCount"; Text "[None]"; Rel "clear"]
                ]
                TR [] -< [
                    TD [Text "Health"]
                    TD [Id "rightHealth"; Text "[None]"; Rel "clear"]
                ]
                TR [] -< [
                    TD [Text "Armour"]
                    TD [Id "rightArmour"; Text "[None]"; Rel "clear"]
                ]
                TR [] -< [
                    TD [Text "Attack Value"]
                    TD [Id "rightAtkVal"; Text "[None]"; Rel "clear"]
                ]
                TR [] -< [
                    TD [Text "Weapon"]
                    TD [Id "rightWeapon"; Text "[None]"; Rel "clear"]
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
                                Text "Close"] |>! OnClick (fun _ _ -> hideModal())
                        saveItemButton
                    ]
                ]
            ]
        ]

    let addItemsToAsk (items : ('a * string) list) (callback : 'a -> unit) =
        JQuery.Of(askItems.Dom).Empty().Ignore
        items |> List.iter(fun (itemGuid, itemName) ->
            let newItem = LI [Attr.Class "list-group-item"; Text itemName]
            askItems.Append(newItem)
            newItem |>! OnClick (fun evt m ->
                JQuery.Of(askItems.Dom).Children("li").RemoveClass("active").Ignore             
                JQuery.Of(evt.Dom).AddClass("active").Ignore
                JQuery.Of(saveItemButton.Dom)
                    .Unbind("click")
                    .Click(fun _ _ -> hideModal(); callback itemGuid).Ignore
            ) |> ignore)
        JQuery.Of(saveItemButton.Dom).Unbind("click").Click(fun _ _ -> hideModal()).Ignore
        showModal()

    let addItemsToAskForPosition (items : string list) (callback : int -> unit) =
        JQuery.Of(askItems.Dom).Empty().Ignore
        for (idx, item) in items |> List.mapi(fun i it -> i, it) do
            let pos = LI [Attr.Class "list-group-item"; Text "->"]
            pos |>! OnClick (fun evt m ->
                JQuery.Of(askItems.Dom).Children("li").RemoveClass("active").Ignore
                JQuery.Of(evt.Dom).AddClass("active").Ignore
                JQuery.Of(saveItemButton.Dom)
                    .Unbind("click")
                    .Click(fun _ _ -> hideModal(); callback idx)
                    .Ignore
            ) |> ignore
            askItems.Append(pos)
            let newItem = LI [Attr.Class "list-group-item"; Text item]
            askItems.Append(newItem)

        // Add item for last position
        let pos = LI [Attr.Class "list-group-item"; Text "->"]
        pos |>! OnClick (fun evt m ->
            JQuery.Of(askItems.Dom).Children("li").RemoveClass("active").Ignore
            JQuery.Of(evt.Dom).AddClass("active").Ignore
            JQuery.Of(saveItemButton.Dom)
                .Unbind("click")
                .Click(fun _ _ -> hideModal(); callback items.Length)
                .Ignore
        ) |> ignore
        askItems.Append(pos)
        JQuery.Of(saveItemButton.Dom).Unbind("click").Click(fun _ _ -> hideModal()).Ignore
        showModal()

    let cardTemplateDiv (playable : bool) (card : CardOnHand) (owner : Player) =

        let cardImgUrl = "http://wow.zamimg.com/images/hearthstone/cards/enus/medium/" + card.Card.Id + ".png"
        let previewButton =
            JQuery.Of("<button />")
                .AddClass("btn btn-default")
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
                            doAsync (Remoting.DoesCardNeedTarget card.Card.Name)
                                (fun needTarget ->
                                    match needTarget with
                                    | false ->
                                        match card.Card.Type with
                                        | "Minion" ->
                                            addItemsToAskForPosition (owner.MinionPosition |> List.map(fun e -> e.Card.Name))
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
                                                        addItemsToAskForPosition (owner.MinionPosition |> List.map(fun e -> e.Card.Name))
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
            else item.AddClass("btn btn-default")

        let cardInfo =
            // TODO: change label color when real cost <> card cost
            let cost = 
                let costLabel = JQuery.Of("<div />").AddClass("col-xs-1").Text(card.Cost.ToString())
                if card.Cost > card.Card.Cost.Value then
                    costLabel.AddClass("label label-danger")
                else if card.Cost < card.Card.Cost.Value then
                    costLabel.AddClass("label label-success")
                else
                    costLabel.AddClass("label label-primary")
            let name = 
                let nameLabel = JQuery.Of("<div />").AddClass("col-xs-8").Text(card.Card.Name)
                match card.Card.Rarity with
                | Some "Legendary" -> nameLabel.Css("color", "orange")
                | Some "Epic" -> nameLabel.Css("color", "violet")
                | Some "Rare" -> nameLabel.Css("color", "blue")
                | Some "Common" -> nameLabel.Css("color", "black")
                | _ -> nameLabel.Css("color", "gray")

            
            let atk =
                if card.Card.Type = "Minion" || card.Card.Type = "Weapon" then
                    JQuery.Of("<div />").AddClass("col-xs-1").AddClass("label label-default").Text(if card.Card.Attack.IsSome then card.Card.Attack.Value.ToString() else "")
                else
                    JQuery.Of("<div />")
            let hpOrDu =
                if card.Card.Type = "Minion" then
                    JQuery.Of("<div />").AddClass("col-xs-1").AddClass("label label-default").Text(if card.Card.Health.IsSome then card.Card.Health.Value.ToString() else "")
                else if card.Card.Type = "Weapon" then
                    JQuery.Of("<div />").AddClass("col-xs-1").AddClass("label label-default").Text(if card.Card.Durability.IsSome then card.Card.Durability.Value.ToString() else "")
                else
                    JQuery.Of("<div />")
            JQuery.Of("<h5/>").AddClass("row").Append(cost).Append(name).Append(atk).Append(hpOrDu)

        let newItem = LI []
        JQuery.Of(newItem.Dom)
            .AddClass("list-group-item")
            .Append(JQuery.Of("<div />").AddClass("row").AddClass("clearfix")
                .Append(JQuery.Of("<div />").AddClass("col-xs-4").Append(previewButton).Append(playCardButton))
                .Append(JQuery.Of("<div />").AddClass("col-xs-8").Append(cardInfo))
            ).Ignore
        newItem

    let minionTemplateDiv (minion : Minion) =
        let cardImgUrl = "http://wow.zamimg.com/images/hearthstone/cards/enus/medium/" + minion.Card.Id + ".png"
        let previewButton =
            JQuery.Of("<button />")
                .AddClass("btn")
                .AddClass("btn-default")
                .Text("Image")
                .Click(fun _ _ ->
                    notifyImage(cardImgUrl)
                )
        let attackButton = 
            JQuery.Of("<button />")
                .Attr("type", "button")
                .AddClass("btn")
                .AddClass("btn-default")
                .Text("Attack")
                .Click(fun _ _ ->
                    ()
                )

        let minionInfo =
            let name = 
                let nameLabel = JQuery.Of("<div />").AddClass("col-xs-8").Text(minion.Card.Name)
                match minion.Card.Rarity with
                | Some "Legendary" -> nameLabel.Css("color", "orange")
                | Some "Epic" -> nameLabel.Css("color", "violet")
                | Some "Rare" -> nameLabel.Css("color", "blue")
                | Some "Common" -> nameLabel.Css("color", "black")
                | _ -> nameLabel.Css("color", "gray")

            let atk = 
                let elem = JQuery.Of("<div />").AddClass("col-xs-1").Text(minion.AttackValue.ToString())
                if minion.AttackValue > minion.Card.Attack.Value then
                    elem.AddClass("label label-success")
                else
                    elem.AddClass("label label-default")
            let hp =
                let elem = JQuery.Of("<div />").AddClass("col-xs-1").Text(minion.CurrentHealth.ToString())
                if minion.CurrentHealth > minion.Card.Health.Value then
                    elem.AddClass("label label-success")
                else if minion.CurrentHealth < minion.MaxHealth then
                    elem.AddClass("label label-danger")
                else
                    elem.AddClass("label label-default")
            JQuery.Of("<h5/>").AddClass("row").Append(name).Append(atk).Append(hp)

        let newItem = LI []
        JQuery.Of(newItem.Dom)
            .AddClass("list-group-item")
            .Append(JQuery.Of("<div />").AddClass("row").AddClass("clearfix")
                .Append(JQuery.Of("<div />").AddClass("col-xs-4").Append(previewButton).Append(attackButton))
                .Append(JQuery.Of("<div />").AddClass("col-xs-8").Append(minionInfo))
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
            let isActive = currentGame.ActivePlayerGuid = player.Guid
            if isActive then
                JQuery.Of("#" + playerStr + "Panel").AddClass("panel-success").Ignore
                JQuery.Of("#" + playerStr + "EndTurnBtn")
//                    .RemoveClass("btn-success")
                    .AddClass("btn-warning")
                    .RemoveAttr("disabled").Ignore
                JQuery.Of("#" + playerStr + "ProggressbarUnused").AddClass("progress-bar-striped active").Ignore
            else
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
            JQuery.Of("#" + playerStr + "Health").Text(player.HeroCharacter.Hp.ToString()).Ignore
            if player.ActiveWeapon.IsSome then
                JQuery.Of("#" + playerStr + "Weapon").Text("").Ignore
                JQuery.Of("<div />").AddClass("row")
                    .Append(JQuery.Of("<div />").AddClass("col-xs-9").Text(player.ActiveWeapon.Value.Card.Name))
                    .Append(JQuery.Of("<div />").AddClass("col-xs-1").AddClass("label label-default").Text(player.ActiveWeapon.Value.Attack.ToString()))
                    .Append(JQuery.Of("<div />").AddClass("col-xs-1").AddClass("label label-default").Text(player.ActiveWeapon.Value.Durability.ToString()))
                    .AppendTo(JQuery.Of("#" + playerStr + "Weapon")).Ignore
                JQuery.Of("#" + playerStr + "Weapon").Parent().FadeIn().Ignore
            else
                JQuery.Of("#" + playerStr + "Weapon").Parent().FadeOut().Ignore

            if player.HeroCharacter.Armour = 0 then
                JQuery.Of("#" + playerStr + "Armour").Parent().FadeOut().Ignore
            else
                JQuery.Of("#" + playerStr + "Armour").Text(player.HeroCharacter.Armour.ToString()).Parent().FadeIn().Ignore
            if player.HeroCharacter.AttackValue = 0 then
                JQuery.Of("#" + playerStr + "AtkVal").Parent().FadeOut().Ignore
            else
                JQuery.Of("#" + playerStr + "AtkVal").Text(player.HeroCharacter.AttackValue.ToString()).Parent().FadeIn().Ignore
            JQuery.Of("#" + playerStr + "UseHeroPowerBtn").FadeIn().Ignore
            match (not player.HeroPowerUsed) && isActive && (player.HeroPower.Cost <= player.CurrentMana) with
            | true -> JQuery.Of("#" + playerStr + "UseHeroPowerBtn").AddClass("btn-success").RemoveAttr("disabled").Ignore
            | false -> JQuery.Of("#" + playerStr + "UseHeroPowerBtn").RemoveClass("btn-success").Attr("disabled", "true").Ignore
            
            updateMana player.CurrentMana player.MaxMana player.Guid

            JQuery.Of("#" + playerStr + "Hand").Children().Remove().Ignore
            player.Hand |> List.iter(fun card ->
                let newCard = cardTemplateDiv (card.Cost <= player.CurrentMana) card player
                JQuery.Of("#" + playerStr + "Hand").Append(JQuery.Of(newCard.Dom)).Ignore
            )

            JQuery.Of("#" + playerStr + "Board").Children().Remove().Ignore
            player.MinionPosition |> List.iter(fun minion ->
                let minion = minionTemplateDiv minion
                JQuery.Of("#" + playerStr + "Board").Append(JQuery.Of(minion.Dom)).Ignore
            )

    let clearGame () =
        currentGame.Clear()
        JQuery.Of("[rel='clear']").Text("[None]").Ignore
        JQuery.Of("[rel='emptyChildren']").Children().Remove().Ignore
        [ "left"; "right" ]
        |> List.iter(fun playerStr -> 
            JQuery.Of("#" + playerStr + "Panel").RemoveClass("panel-success").Ignore
            JQuery.Of("#" + playerStr + "EndTurnBtn")
                .RemoveClass("btn-success")
                .RemoveClass("btn-warning")
                .Attr("disabled", "true")
                .Ignore
            JQuery.Of("#" + playerStr + "UseHeroPowerBtn").FadeOut().Ignore
            JQuery.Of("#" + playerStr + "ProggressbarUnused").Css("width", "0%").Ignore
            JQuery.Of("#" + playerStr + "ProggressbarUsed").Css("width", "0%").Ignore
            )

    let newGame gameGuid =
        clearGame()
        currentGame.Guid <- gameGuid
        JQuery.Of(gameGuidLabel.Dom).Text(gameGuid.value + " " + (string currentGame.LastChanged)).Ignore
        match Remoting.GetGameLastChangedTime currentGame.Guid with
        | Success time -> currentGame.LastChanged <- time
        | Error msg -> notifyError(msg)  
        
    let updatePlayers () =
        if currentGame.Exist() then
            match Remoting.GetGameLastChangedTime currentGame.Guid with
            | Error msg -> JavaScript.Log(msg)
            | Success time ->
                if time > currentGame.LastChanged then
                    doAsync (Remoting.GetActivePlayerGuid(currentGame.Guid))
                        (fun ret -> 
                            match ret with
                            | Success guid -> 
                                currentGame.ActivePlayerGuid <- guid
                                currentGame.LastChanged <- time
                                JQuery.Of(gameGuidLabel.Dom).Text(currentGame.Guid.value + " " + (string currentGame.LastChanged)).Ignore             
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

    let backgroundTask = 
        JavaScript.SetInterval
            (fun _ ->
                updatePlayers()
            )
            1000

    let setupButton =

        JQuery.Of(startGameButton.Dom)
            .Click(fun _ _ ->
                if currentGame.Exist() then
                    doAsync (Remoting.StartGame currentGame.Guid)
                        (fun ret ->
                            match ret with
                            | Success msg -> notifySuccess(msg)
                            | Error msg -> notifyError(msg)
                        )
            ).Ignore
            
        JQuery.Of(newGameButton.Dom)
            .Click(fun _ _ ->
                doAsync (Remoting.NewGame())
                    (fun res ->
                        match res with
                        | Success gameGuid -> newGame gameGuid
                        | Error msg -> notifyError(msg)))
            .Ignore

        JQuery.Of(registerPlayerButton.Dom)
            .Click(fun _ _ ->
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
            ).Ignore

        JQuery.Of(leftPlayerEndTurnButton.Dom)
            .Attr("disabled", "true")
            .Click(fun _ _ ->
                doAsync (Remoting.EndTurn currentGame.LeftPlayer.Guid currentGame.Guid)
                    (fun ret ->
                        match ret with
                        | Success msg -> notifySuccess(msg)
                        | Error msg -> notifyError(msg)
                    )
                )
            .Ignore

        JQuery.Of(rightPlayerEndTurnButton.Dom)
            .Attr("disabled", "true")
            .Click(fun _ _ ->
                doAsync (Remoting.EndTurn currentGame.RightPlayer.Guid currentGame.Guid)
                    (fun ret ->
                        match ret with
                        | Success msg -> notifySuccess(msg)
                        | Error msg -> notifyError(msg)
                    )
                )
            .Ignore
            
            
        JQuery.Of(leftPlayerUseHeroPowerButton.Dom)
            .Attr("disabled", "true")
            .Hide()
            .Click(fun _ _ -> useHeroPower currentGame.LeftPlayer).Ignore

        JQuery.Of(rightPlayerUseHeroPowerButton.Dom)
            .Attr("disabled", "true")
            .Hide()
            .Click(fun _ _ -> useHeroPower currentGame.RightPlayer).Ignore

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
                    Div [Attr.Class "panel panel-default"; Id "leftPanel"] -< [
                        Div [Attr.Class "panel-heading"] -- H3 [Attr.Class "panel-title"; Text "Left Player"]
                        Div [Attr.Class "panel-body"] -< [ 
                            leftPlayerEndTurnButton
                            leftPlayerUseHeroPowerButton
                            leftPlayerInfoTable
                            leftPlayerManaBar
                        ]
                    ]
                    leftPlayerHand
                ]
                Div [Attr.Class "col-md-6 column"] -< [
                    Div [Attr.Class "panel panel-default"; Id "rightPanel"] -< [
                        Div [Attr.Class "panel-heading"] -- H3 [Attr.Class "panel-title"; Text "Right Player"]
                        Div [Attr.Class "panel-body"] -< [ 
                            rightPlayerEndTurnButton
                            rightPlayerUseHeroPowerButton
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
        ]

    let About () =
        Div [Attr.Class "jumbotron"] -< [
            H3 [Text "@Tue AKIO 2014"]
        ]
