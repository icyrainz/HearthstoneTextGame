namespace HearthstoneTextGame

module Card =

    let basicTotems =
        [ "Healing Totem"
          "Searing Totem"
          "Stoneclaw Totem"
          "Wrath of Air Totem"
        ] 

    let parseEntityJsonToCard (entity : EntityJson.T) =
        { Id = entity.Id
          Name = entity.Name
          Type = entity.Type
          Rarity = entity.Rarity
          Race = entity.Race
          CardClass = entity.PlayerClass
          Cost = entity.Cost
          Attack = entity.Attack
          Health = entity.Health
          Durability = entity.Durability
          Text = entity.Text
          Mechanics = entity.Mechanics }

    let playableCards =
        EntityJson.All
        |> List.filter(fun e ->
            e.Collectible.IsSome && e.Collectible.Value
            && (e.Type = "Minion" || e.Type = "Spell" || e.Type = "Weapon"))
        |> List.map(fun e -> parseEntityJsonToCard(e))

    let getRandomPlayableCard (hero : string) =
        let eligibleCards = playableCards |> List.filter(fun e -> e.CardClass.IsNone)
        List.nth eligibleCards <| Utility.rngNext(eligibleCards.Length)

    let getCardByExactName (name : string) =
        playableCards
        |> List.find(fun e -> e.Name = name)

    let getEntityByExactName (name : string) =
        let entity =
            EntityJson.All
            |> Seq.filter(fun e -> e.Name = name) |> Seq.toList
        if entity.Length = 0 then None
        else
            Some <| parseEntityJsonToCard entity.Head

    let getCardById (id : string) =
        playableCards
        |> List.find(fun e -> e.Id = id)

    let getCardIdsByNames (nameList : string list) =
        nameList |> List.map (fun name -> (getCardByExactName name).Id)

    let TheCoin =
        { (EntityJson.All 
           |> List.find(fun e -> e.Name = "The Coin" && e.Type = "Spell")
           |> parseEntityJsonToCard)
           with Cost = Some 0
        }