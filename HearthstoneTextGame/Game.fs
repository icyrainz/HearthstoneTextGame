namespace HearthstoneTextGame


module Game =

    EntityJson.preload()

    let initPlayer name =
        { Player.Empty with Name = name
                            HeroClass = "Mage"
                            HeroPower = Hero.getHeroPower "Mage" true
                            Deck = Deck.getRandomDeck "Mage" }

    let initPlayerWithDeck name deck = 
        { Player.Empty with Name = name
                            HeroClass = deck.DeckClass
                            HeroPower = Hero.getHeroPower deck.DeckClass true
                            Deck = deck }               

    let getPlayer (playerGuid : Guid) (game : GameSession) =
        let result = game.Players |> List.filter(fun e -> e.Guid = playerGuid)
        match result |> List.isEmpty with
        | true -> None
        | false -> Some (result |> List.head)

    let getOpponent (playerGuid : Guid) (game : GameSession) =
        let result = game.Players |> List.filter(fun e -> e.Guid <> playerGuid)
        match result |> List.isEmpty with
        | true -> None
        | false -> Some (result |> List.head)

    let addPlayer (player : Player) (game : GameSession) =
        if game.PlayerCount = 2 then None
        else Some { game with Players = player :: game.Players }

    let registerPlayer (playerName : string) (deck : Deck) (game : GameSession) =
        let newPlayer = initPlayerWithDeck playerName deck
        addPlayer newPlayer game |> Option.map(fun newGame -> newPlayer, newGame)

    let registerRandomDeckPlayer (playerName : string) (game : GameSession) =
        let newPlayer = initPlayer playerName
        addPlayer newPlayer game |> Option.map(fun newGame -> newPlayer, newGame)

    let registerRandomDeckPlayerWithClass (playerName : string) (playerClass : string) (game : GameSession) =
        let deck = Deck.getRandomDeck(playerClass)
        registerPlayer playerName deck game

    let updatePlayer (items : ICharacter list) (player : Player) =
        let newPlayer = ref player
        items
        |> List.iter(fun item ->
            if item.Guid = player.Face.Guid then
                newPlayer := { !newPlayer with Face = (item :?> Face) }
            else
                let newMinionPosition =
                    player.MinionPosition |> List.map(fun minion ->
                        if item.Guid = minion.Guid then (item :?> Minion) else minion)
                newPlayer := { !newPlayer with MinionPosition = newMinionPosition })
        !newPlayer


    let updatePlayerToGame (player : Player) (game : GameSession) =
        let newPlayers =
            game.Players |> List.map (fun e ->
                if e.Guid = player.Guid then player
                else e)
        { game with Players = newPlayers }

    let updateICharToGame (items : ICharacter list) (game : GameSession) =
        let newPlayers = game.Players |> List.map (fun e -> updatePlayer items e)
        { game with Players = newPlayers }

    let playMinion (minion : Minion) (pos : int) (player : Player) (game : GameSession) =
        if pos > player.MinionPosition.Length || pos < 0 then
            None
        else 
            let newMinionPosition = Utility.insert minion pos player.MinionPosition
            // TODO: trigger battlecry
            let newPlayer = { player with MinionPosition = newMinionPosition }
            Some <| updatePlayerToGame newPlayer game

    let updateMinionToDie (game : GameSession) =
        { game with 
            Players =
                game.Players 
                |> List.map(fun player ->
                    let minions = 
                        player.MinionPosition
                        |> List.choose(fun m ->
                            if m.CurrentHealth <= 0 then
                                // TODO: process Deathrattle
                                None
                            else
                                Some m
                        )
                    { player with MinionPosition = minions }
                )
        }

    let playWeapon (weapon : Weapon) (player : Player) (game : GameSession) =
        let newAttackValue = 
            if player.ActiveWeapon.IsSome then
                player.Face.AttackValue - player.ActiveWeapon.Value.Attack + weapon.Attack
            else
                player.Face.AttackValue + weapon.Attack
        let newPlayer = { player with ActiveWeapon = Some { weapon with CanAttack = true }
                                      Face = { player.Face with AttackValue = newAttackValue } }
        Some <| updatePlayerToGame newPlayer game

    let drawCard (player : Player) (game : GameSession) =
        if player.Deck.RemainingCardsCount > 0 then  
            let cardDraw, remainDeck = Deck.drawCardFromDeck player.Deck
            let cardDrawOnHand = CardOnHand.Parse(cardDraw)    
            let newHand =
                if player.Hand.Length = Config.maxCardsOnHand then player.Hand
                // Report too many cards on hand
                else
                    Utility.insert cardDrawOnHand player.Hand.Length player.Hand
            let newPlayer = { player with Hand = newHand; Deck = remainDeck }
            let newGame = updatePlayerToGame newPlayer game
            Some cardDraw, newGame
        else
            None, game

    let useHeroPower (player : Player) (target : ICharacter option) (game : GameSession) =
        if player.CurrentMana < player.HeroPower.Cost then None
        else if player.HeroPowerUsed then None
        else
            let newPlayer = { player with HeroPowerUsed = true; CurrentMana = player.CurrentMana - player.HeroPower.Cost }
            let newGame = updatePlayerToGame newPlayer game
            match player.HeroPower.Id with
            | "CS2_034" (* Fireblast *) ->
                let newTarget = target.Value.GetDamage(1)
                updateICharToGame [newTarget] newGame |> updateMinionToDie |> Some
            | "CS2_017" (* Shapeshift *) ->
                let armour = newPlayer.Face.Armour + 1
                let attackVal = newPlayer.Face.AttackValue + 1
                let newHeroChar = { newPlayer.Face with Armour = armour; AttackValue = attackVal }
                let aPlayer = newPlayer |> updatePlayer [newHeroChar]
                Some <| updatePlayerToGame aPlayer newGame
            | "CS2_049" (* Totemic Call *) ->
                let newTotems =
                    Card.basicTotems
                    |> List.filter(fun e -> 
                        newPlayer.MinionPosition 
                        |> List.exists(fun m -> m.Card.Name = e) |> not)
                    |> List.map(fun e -> Minion.Parse(Card.getEntityByExactName(e).Value).Value)
                if newTotems.Length = 0 then None
                else playMinion (fst <| Utility.removeRandomElem newTotems) newPlayer.MinionPosition.Length newPlayer newGame          
            | "DS1h_292" (* Steady Shot *) ->
                let opponent = getOpponent newPlayer.Guid newGame
                if opponent.IsNone then None
                else
                    let newTarget = (opponent.Value.Face :> ICharacter).GetDamage(2)
                    let aPlayer = opponent.Value |> updatePlayer [newTarget]
                    Some <| updatePlayerToGame aPlayer newGame
            | "CS2_101" (* Reinforce *) ->
                let card = Card.getEntityByExactName("Silver Hand Recruit").Value
                let token = Minion.Parse(card)
                if token.IsNone then None
                else playMinion token.Value newPlayer.MinionPosition.Length newPlayer newGame
            | "CS2_083b" (* Dagger Mastery *) ->
                let knife = Weapon.Parse(Card.getEntityByExactName("Wicked Knife").Value)
                if knife.IsNone then None
                else playWeapon knife.Value newPlayer newGame
            | "CS1h_001" (* Lesser Heal *) ->
                let newTarget = target.Value.GetHeal(2)
                updateICharToGame [newTarget] newGame |> updateMinionToDie |> Some
            | "CS2_056" (* Life Tap *) ->
                let _, newGame = drawCard newPlayer newGame
                let newFace = (newPlayer.Face :> ICharacter).GetDamage(2) :?> Face
                Some <| updateICharToGame [newFace] newGame
            | "CS2_102" (* Armour Up! *) ->
                let newArmour = newPlayer.Face.Armour + 2
                let newHeroChar = { newPlayer.Face with Armour = newArmour }
                let aPlayer = newPlayer |> updatePlayer [newHeroChar]
                Some <| updatePlayerToGame aPlayer newGame
            | _ ->
                None

    let findIChar (guid : Guid) (game : GameSession) ifHero ifMinion =
        let found = 
            game.Players 
            |> List.choose (fun e ->
                if e.Face.Guid = guid then
                    ifHero e.Face
                    Some (e.Face :> ICharacter)
                else
                    e.MinionPosition |> List.tryFind (fun m -> m.Guid = guid)
                    |> Option.map(fun minion ->
                        ifMinion minion
                        minion :> ICharacter
                    )
            )

        if found.Length = 1 then Some found.Head
        else None

    let getOwnerPlayer (obj : ICharacter) (game : GameSession) =
        game.Players 
        |> List.filter (fun e ->
            e.Face.Guid = obj.Guid ||
            e.MinionPosition |> List.exists (fun m -> m.Guid = obj.Guid)
        )
        |> List.head
       

    let findTarget (targetType : TargetType) (player : Player) (game : GameSession) =
        getOpponent player.Guid game |> Option.map(fun opponent ->
            match targetType with
            | AnyTarget Any ->
                [ player.Face.Guid
                  opponent.Face.Guid ]
                |> List.append(player.MinionPosition |> List.map(fun e -> e.Guid))
                |> List.append(opponent.MinionPosition |> List.map(fun e -> e.Guid))
            | AnyTarget Friendly ->
                [ player.Face.Guid ]
                |> List.append(player.MinionPosition |> List.map(fun e -> e.Guid))
            | AnyTarget Enemy ->
                [ opponent.Face.Guid ]
                |> List.append(opponent.MinionPosition |> List.map(fun e -> e.Guid))
            | MinionTarget Any ->
                [ ]
                |> List.append(player.MinionPosition |> List.map(fun e -> e.Guid))
                |> List.append(opponent.MinionPosition |> List.map(fun e -> e.Guid))
            | MinionTarget Friendly ->
                [ ]
                |> List.append(player.MinionPosition |> List.map(fun e -> e.Guid))
            | MinionTarget Enemy ->
                [ ]
                |> List.append(opponent.MinionPosition |> List.map(fun e -> e.Guid))
        )

    let findTargetForHeroPower (player : Player) (game : GameSession) =
        Hero.heroPowers |> List.tryFind(fun e -> e = player.HeroPower)
        |> Option.bind(fun heroPower ->
            heroPower.Target |> Option.bind(fun target -> 
                findTarget target player game
            )
        )

    let getTargetForCard (cardName : string) =
        match cardName with
        | "Perdition's Blade" -> Some (AnyTarget(Any), (fun ((target : ICharacter), game) -> updateICharToGame [target.GetDamage(1)] game))
        | _ -> None

    let findTargetForCard (card : Card) (player : Player) (game : GameSession) =
        getTargetForCard card.Name |> Option.bind(fun (target, action) ->
            findTarget target player game
        )

    let playCard (card : CardOnHand) (pos : int option) (target : ICharacter option) (player : Player) (game : GameSession) =
        if player.CurrentMana < card.Cost then None
        else // TODO: DO SOMETHING TO TARGET
            match card.Card.Type with
            | "Spell" -> None
            | "Weapon" ->
                let mutable newPlayer = { player with CurrentMana = player.CurrentMana - card.Cost
                                                      Hand = player.Hand |> List.filter(fun c -> c <> card)
                                        }
                let mutable newGame =
                    if target.IsSome then
                        getTargetForCard card.Card.Name 
                        |> Option.map(fun (_, action) ->
                            action (target.Value, game)
                        )
                    else
                        None
                if newGame.IsSome then
                    (playWeapon (Weapon.Parse(card.Card).Value) newPlayer newGame.Value)
                else
                    (playWeapon (Weapon.Parse(card.Card).Value) newPlayer game)
            | "Minion" ->
                if pos.IsNone || player.MinionPosition.Length = Config.maxMinionsOnBoard then None
                else
                    let mutable newPlayer = { player with CurrentMana = player.CurrentMana - card.Cost
                                                          Hand = player.Hand |> List.filter(fun c -> c <> card)
                                            }
                    (playMinion (Minion.Parse(card.Card).Value) pos.Value newPlayer game)
            | _ -> None

    let startGame (game : GameSession) =
        if game.PlayerCount <> Config.maxNumberOfPlayers || game.CurrentPhase <> NotStarted then None
        else
            let startPlayer = Utility.rngNext(Config.maxNumberOfPlayers) |> List.nth game.Players
            let secondPlayer = (getOpponent startPlayer.Guid game).Value
            let newSecondPlayer = { secondPlayer with Hand = CardOnHand.Parse(Card.TheCoin) :: secondPlayer.Hand}
            let newStartPlayer = { startPlayer with CurrentMana = 1; MaxMana = 1 }
            let mutable newGame = updatePlayerToGame newStartPlayer game
            newGame <- updatePlayerToGame newSecondPlayer newGame
            let _, newGameWithDrawCard = drawCard newStartPlayer newGame
            Some { newGameWithDrawCard with ActivePlayerGuid = newStartPlayer.Guid; CurrentPhase = Playing }

            

    let endTurn (game : GameSession) =
        if game.CurrentPhase <> Playing then None
        else
            let mutable newGame = game
            // Activate trigger in endTurn of current Player
            // DO


            // Reset hero power
            let mutable tempPlayer = (getPlayer game.ActivePlayerGuid game).Value
            tempPlayer <- { tempPlayer with HeroPowerUsed = false }
            newGame <- updatePlayerToGame tempPlayer newGame
            
            // Reset hero character atk value to weapon atk
            tempPlayer <- { tempPlayer with 
                                Face = { tempPlayer.Face with 
                                                    AttackValue =
                                                        if tempPlayer.ActiveWeapon.IsSome then
                                                            tempPlayer.ActiveWeapon.Value.Attack
                                                        else
                                                            0 
                                                }
                          }
            newGame <- updatePlayerToGame tempPlayer newGame

            // Set activePlayer to opponent and go to next phase
            let opp = (getOpponent newGame.ActivePlayerGuid newGame).Value
            newGame <- { newGame with ActivePlayerGuid = opp.Guid }


            // Activate trigger in beginTurn of Opp
            // DO
        
            // Draw card
            let _, aGame = drawCard (getPlayer newGame.ActivePlayerGuid newGame).Value newGame
            newGame <- aGame

            // Increase mana for ActivePlayer
            tempPlayer <- (getPlayer newGame.ActivePlayerGuid newGame).Value
            let maxMana = if tempPlayer.MaxMana = 10 then 10 else tempPlayer.MaxMana + 1
            tempPlayer <- { tempPlayer with MaxMana = maxMana
                                            CurrentMana = maxMana }

            // Set player and go to next phase
            Some <| updatePlayerToGame tempPlayer newGame

    