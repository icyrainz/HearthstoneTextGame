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
        if game.PlayerCount = Config.maxNumberOfPlayers then None
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
            if item.Guid = (!newPlayer).Face.Guid then
                newPlayer := { !newPlayer with Face = (item :?> Face) }
            else
                let newMinions =
                    (!newPlayer).Minions |> List.map(fun minion ->
                        if item.Guid = minion.Guid then (item :?> Minion) else minion)
                newPlayer := { !newPlayer with Minions = newMinions })
        !newPlayer

    // TODO: Move this to inside Minion
    let updateThings (game : GameSession) =
        { game with 
            Players =
                game.Players 
                |> List.map(fun player ->
                    let minions = 
                        player.Minions
                        |> List.choose(fun m ->
                            if m.CurrentHealth <= 0 then
                                // TODO: process minion deathrattle
                                None
                            else
                                Some m
                        )
                    let weapon =
                        if player.Face.Weapon.IsSome && player.Face.Weapon.Value.Durability <= 0 then
                            // TODO: process weapon deathrattle
                            None
                        else
                            player.Face.Weapon
                    { player with Minions = minions
                                  Face = { player.Face with Weapon = weapon }
                    }
                )
        }

    let updatePlayerToGame (player : Player) (game : GameSession) =
        let newPlayers =
            game.Players |> List.map (fun e ->
                if e.Guid = player.Guid then player
                else e)
        { game with Players = newPlayers } |> updateThings

    let updateICharToGame (items : ICharacter list) (game : GameSession) =
        let newPlayers = game.Players |> List.map (fun e -> updatePlayer items e)
        { game with Players = newPlayers } |> updateThings

    let findIChar (guid : Guid) (game : GameSession) ifHero ifMinion =
        let found = 
            game.Players 
            |> List.choose (fun e ->
                if e.Face.Guid = guid then
                    ifHero e.Face
                    Some (e.Face :> ICharacter)
                else
                    e.Minions |> List.tryFind (fun m -> m.Guid = guid)
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
            e.Minions |> List.exists (fun m -> m.Guid = obj.Guid)
        )
        |> List.head
       
    let findTargetsFromType (targetType : TargetType) (playerGuid : Guid) (game : GameSession) =
        getPlayer playerGuid game |> Option.bind(fun player ->
            getOpponent playerGuid game |> Option.map(fun opponent ->
                match targetType with
                | AnyTarget Any ->
                    [ player.Face.Guid
                      opponent.Face.Guid ]
                    |> List.append(player.Minions |> List.map(fun e -> e.Guid))
                    |> List.append(opponent.Minions |> List.map(fun e -> e.Guid))
                | AnyTarget Friendly ->
                    [ player.Face.Guid ]
                    |> List.append(player.Minions |> List.map(fun e -> e.Guid))
                | AnyTarget Enemy ->
                    [ opponent.Face.Guid ]
                    |> List.append(opponent.Minions |> List.map(fun e -> e.Guid))
                | MinionTarget Any ->
                    [ ]
                    |> List.append(player.Minions |> List.map(fun e -> e.Guid))
                    |> List.append(opponent.Minions |> List.map(fun e -> e.Guid))
                | MinionTarget Friendly ->
                    [ ]
                    |> List.append(player.Minions |> List.map(fun e -> e.Guid))
                | MinionTarget Enemy ->
                    [ ]
                    |> List.append(opponent.Minions |> List.map(fun e -> e.Guid))
                | FaceTarget Any ->
                    [ player.Face.Guid
                      opponent.Face.Guid ]
                | FaceTarget Friendly ->
                    [ player.Face.Guid ]
                | FaceTarget Enemy ->
                    [ opponent.Face.Guid ]
            )
        )
    
    let getTargetForCard (cardId : string) =
        match cardId with
        | "EX1_133" (* Perdition's Blade *) -> 
            Some (AnyTarget(Any)), 
            (fun (((target : ICharacter option), playerGuid : Guid), game) -> 
                updateICharToGame [target.Value.GetDamage(1)] game
            )
        | "GAME_005" (* The Coin *) -> 
            None, 
            (fun ((_, playerGuid), game) ->
                match getPlayer playerGuid game with
                | Some player ->
                    let newPlayer = { player with CurrentMana = player.CurrentMana + 1 }
                    updatePlayerToGame newPlayer game
                | None -> game
            )
        | "EX1_319" (* Flame Imp *) -> 
            None, 
            (fun ((_, playerGuid), game) ->
                match getPlayer playerGuid game with
                | Some player ->
                    let newPlayer = { player with Face = (player.Face :> ICharacter).GetDamage(3) :?> Face }
                    updatePlayerToGame newPlayer game
                | None -> game
            )
        | _ -> 
            None, 
            (fun (_, game) -> game)

    let findTargetForCard (card : Card) (playerGuid : Guid) (game : GameSession) =
        (fst <| getTargetForCard card.Id) |> Option.bind(fun targetType ->
                findTargetsFromType targetType playerGuid game
        )

    let findTargetToAttack (playerGuid : Guid) (game : GameSession) =
        getOpponent playerGuid game |> Option.map(fun opponent ->
            let taunts = opponent.Minions |> List.filter(fun e -> e.HasTaunt)
            match taunts.Length with
            | EqualZero ->
                opponent.Face.Guid :: (opponent.Minions |> List.map(fun e -> e.Guid))
            | _ ->
                taunts |> List.map(fun e -> e.Guid)
        )  

    let playMinion (minion : Minion) (pos : int) (playerGuid : Guid) (game : GameSession) =
        getPlayer playerGuid game |> Option.bind(fun player ->
            if pos > player.Minions.Length || pos < 0 then
                None
            else 
                let newMinions = Utility.insert minion pos player.Minions
                // TODO: trigger minion battlecry
                // TODO: trigger events when minion is played
                let newPlayer = { player with Minions = newMinions }
                Some <| updatePlayerToGame newPlayer game
        )

    let playWeapon (weapon : Weapon) (playerGuid : Guid) (game : GameSession) =
        getPlayer playerGuid game |> Option.bind(fun player ->
            // TODO: trigger events when weapon is played
            let attackTokens = if weapon.Card.Mechanics |> List.exists(fun e -> e = "Windfury") then 2 else 1
            let newPlayer = { player with Face = { player.Face with Weapon = Some weapon
                                                                    WeaponActivated = true
                                                                    AttackTokens = attackTokens } 
                            }
            Some <| updatePlayerToGame newPlayer game
        )

    let drawCard (playerGuid : Guid) (game : GameSession) =
        match getPlayer playerGuid game with
        | Some player ->
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
        | None -> None, game

    let findTargetForHeroPower (playerGuid : Guid) (game : GameSession) =
        getPlayer playerGuid game |> Option.bind(fun player ->
            Hero.heroPowers |> List.tryFind(fun e -> e = player.HeroPower)
            |> Option.bind(fun heroPower ->
                heroPower.Target |> Option.bind(fun target -> 
                    findTargetsFromType target playerGuid game
                )
            )
        )

    let useHeroPower (playerGuid : Guid) (target : ICharacter option) (game : GameSession) =
        getPlayer playerGuid game |> Option.bind(fun player ->
            if player.CurrentMana < player.HeroPower.Cost then None
            else if player.HeroPowerUsed then None
            else
                let newPlayer = { player with HeroPowerUsed = true; CurrentMana = player.CurrentMana - player.HeroPower.Cost }
                let newGame = updatePlayerToGame newPlayer game
                match player.HeroPower.Id with
                | "CS2_034" (* Fireblast *) ->
                    let newTarget = target.Value.GetDamage(1)
                    updateICharToGame [newTarget] newGame |> Some
                | "CS2_017" (* Shapeshift *) ->
                    let armour = newPlayer.Face.Armour + 1
                    let attackVal = newPlayer.Face.AttackValue + 1
                    let newHeroChar = { newPlayer.Face with Armour = armour
                                                            AttackValue = attackVal }
                    let aPlayer = newPlayer |> updatePlayer [newHeroChar]
                    Some <| updatePlayerToGame aPlayer newGame
                | "CS2_049" (* Totemic Call *) ->
                    let newTotems =
                        Card.basicTotems
                        |> List.filter(fun e -> 
                            newPlayer.Minions 
                            |> List.exists(fun m -> m.Card.Name = e) |> not)
                        |> List.map(fun e -> Minion.Parse(Card.getEntityByExactName(e).Value).Value)
                    if newTotems.Length = 0 then None
                    else playMinion (fst <| Utility.removeRandomElem newTotems) newPlayer.Minions.Length playerGuid newGame          
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
                    else playMinion token.Value newPlayer.Minions.Length playerGuid newGame
                | "CS2_083b" (* Dagger Mastery *) ->
                    let knife = Weapon.Parse(Card.getEntityByExactName("Wicked Knife").Value)
                    if knife.IsNone then None
                    else playWeapon knife.Value playerGuid newGame
                | "CS1h_001" (* Lesser Heal *) ->
                    let newTarget = target.Value.GetHeal(2)
                    updateICharToGame [newTarget] newGame |> Some
                | "CS2_056" (* Life Tap *) ->
                    let _, newGame = drawCard playerGuid newGame
                    let newFace = (newPlayer.Face :> ICharacter).GetDamage(2) :?> Face
                    Some <| updateICharToGame [newFace] newGame
                | "CS2_102" (* Armour Up! *) ->
                    let newArmour = newPlayer.Face.Armour + 2
                    let newHeroChar = { newPlayer.Face with Armour = newArmour }
                    let aPlayer = newPlayer |> updatePlayer [newHeroChar]
                    Some <| updatePlayerToGame aPlayer newGame
                | _ ->
                    None 
        )

    let playCard (card : CardOnHand) (pos : int option) (target : ICharacter option) (playerGuid : Guid) (game : GameSession) =
        getPlayer playerGuid game |> Option.bind(fun player ->
            if player.CurrentMana < card.Cost then None
            else // TODO: DO SOMETHING TO TARGET
                let newPlayer = { player with CurrentMana = player.CurrentMana - card.Cost
                                              Hand = player.Hand |> List.filter(fun c -> c <> card)
                                }
                let mutable newGame = updatePlayerToGame newPlayer game
                newGame <- snd (getTargetForCard card.Card.Id) ((target, playerGuid), newGame)

                match card.Card.Type with
                | "Spell" -> None
                | "Weapon" ->
                    playWeapon (Weapon.Parse(card.Card).Value) playerGuid newGame
                | "Minion" ->
                    if pos.IsNone || player.Minions.Length = Config.maxMinionsOnBoard then None
                    else
                        playMinion (Minion.Parse(card.Card).Value) pos.Value playerGuid newGame
                | _ -> None
        )

    let attackIChar (source : ICharacter) (target : ICharacter) (game : GameSession) =
        match source.CanAttack with
        | true ->
            let newSource, newTarget = source.DoAttack(target)
            Some <| updateICharToGame [newSource; newTarget] game
        | false -> None

    let startGame (game : GameSession) =
        if game.PlayerCount <> Config.maxNumberOfPlayers || game.CurrentPhase <> NotStarted then None
        else
            let startPlayer = Utility.rngNext(Config.maxNumberOfPlayers) |> List.nth game.Players
            Some { game with StartPlayerGuid = startPlayer.Guid; CurrentPhase = Mulligan }

    let getMulligan (playerGuid : Guid) (game : GameSession) =
        if game.CurrentPhase <> Mulligan ||
            game.HasMulliganed |> List.exists(fun e -> e = playerGuid)
        then None
        else
            let numCards = if playerGuid = game.StartPlayerGuid then 3 else 4
            match getPlayer playerGuid game with
            | None -> None
            | Some player -> Some <| Utility.getRandomElem numCards player.Deck.CardIdList

    let endMulligan (chosenCards : string list) (playerGuid : Guid) (game : GameSession) =
        if game.CurrentPhase <> Mulligan then None
        else
            match getPlayer playerGuid game with
            | None -> None
            | Some player ->
                let numCards = if playerGuid = game.StartPlayerGuid then 3 else 4
                let tempList = player.Deck.CardIdList |> List.toArray
                let tempList2 = chosenCards |> List.toArray
                for idx = 0 to tempList.Length - 1 do
                    for jdx = 0 to tempList2.Length - 1 do
                        if tempList2.[jdx] <> "" && tempList.[idx] = tempList2.[jdx] then
                            tempList.[idx] <- ""
                            tempList2.[jdx] <- ""
                let deck = 
                    { player.Deck with 
                        CardIdList = tempList |> Array.filter(fun e -> e <> "") |> Array.toList }
                let hand = chosenCards |> List.map(fun e -> CardOnHand.Parse(Card.getCardById(e)))               
                let newPlayer = { player with Hand = hand; Deck = deck }
                let mutable newGame = updatePlayerToGame newPlayer game
                for i = chosenCards.Length + 1 to numCards do                  
                    let _, newGameWithDrawCard = drawCard player.Guid newGame
                    newGame <- newGameWithDrawCard
                newGame <- { newGame with HasMulliganed = playerGuid :: newGame.HasMulliganed }
                Some newGame

    let afterMulligan (game : GameSession) =
        if game.CurrentPhase <> Mulligan then None
        else
            if game.HasMulliganed.Length = Config.maxNumberOfPlayers then
                let startPlayer = (getPlayer game.StartPlayerGuid game).Value
                let secondPlayer = (getOpponent startPlayer.Guid game).Value
                let newSecondPlayer = { secondPlayer with Hand = CardOnHand.Parse(Card.TheCoin) :: secondPlayer.Hand}
                let newStartPlayer = { startPlayer with CurrentMana = 1; MaxMana = 1 }

                let mutable newGame = updatePlayerToGame newStartPlayer game
                newGame <- updatePlayerToGame newSecondPlayer newGame
                let _, newGameWithDrawCard = drawCard newStartPlayer.Guid newGame
                Some { newGameWithDrawCard with ActivePlayerGuid = newStartPlayer.Guid; CurrentPhase = Playing }
            else
                Some game

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
            
            // Reset hero character atk value & deactivate weapon
            tempPlayer <- { tempPlayer with 
                                Face = { tempPlayer.Face with
                                            WeaponActivated = false
                                            AttackValue = 0
                                            AttackCount = 0 
                                       }
                          }
            newGame <- updatePlayerToGame tempPlayer newGame

            // Reset attack count for all minions
            tempPlayer <- { tempPlayer with
                                Minions = tempPlayer.Minions |> List.map(fun e -> { e with AttackCount = 0 }) 
                          }
            newGame <- updatePlayerToGame tempPlayer newGame

            // Set activePlayer to opponent and go to next phase
            let opp = (getOpponent newGame.ActivePlayerGuid newGame).Value
            newGame <- { newGame with ActivePlayerGuid = opp.Guid }


            // Activate trigger in beginTurn of Opp
            // DO
        
            // Draw card
            let _, aGame = drawCard newGame.ActivePlayerGuid newGame
            newGame <- aGame

            // Increase mana for ActivePlayer
            tempPlayer <- (getPlayer newGame.ActivePlayerGuid newGame).Value
            let maxMana = if tempPlayer.MaxMana = 10 then 10 else tempPlayer.MaxMana + 1
            tempPlayer <- { tempPlayer with MaxMana = maxMana
                                            CurrentMana = maxMana }
            newGame <- updatePlayerToGame tempPlayer newGame

            // Set weapon to active
            tempPlayer <- { tempPlayer with 
                                Face = { tempPlayer.Face with
                                            WeaponActivated = true
                                       }
                          }
            newGame <- updatePlayerToGame tempPlayer newGame

            Some newGame

    