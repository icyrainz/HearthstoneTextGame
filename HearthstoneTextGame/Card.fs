namespace HearthstoneTextGame

open System

module Card =

    let parseEntityJsonToCard (entity : EntityJson.T) =
        { Id = entity.Id
          Name = entity.Name
          Type = entity.Type
          Rarity = entity.Rarity
          Race = entity.Race
          CardClass = entity.PlayerClass
          Cost = entity.Cost.Value
          Attack = entity.Attack
          Health = entity.Health
          Durability = entity.Durability
          Text = entity.Text
          Mechanics = entity.Mechanics |> Array.toList }

    let playableCards =
        EntityJson.All
        |> Seq.filter(fun e ->
            e.Collectible.IsSome && e.Collectible.Value
            && (e.Type = "Minion" || e.Type = "Spell" || e.Type = "Weapon"))
        |> Seq.map(fun e -> parseEntityJsonToCard(e))
        |> Seq.toList

    let getRandomPlayableCard (hero : string) =
        let eligibleCards = playableCards |> List.filter(fun e -> e.CardClass.IsNone || e.CardClass.Value = hero)
        List.nth eligibleCards <| Random().Next(eligibleCards |> List.length)

    let getRandomTotem () =
        let totems = 
            EntityJson.All 
            |> Seq.filter(fun e -> e.Race = Some "Totem")
            |> Seq.map(fun e -> parseEntityJsonToCard(e)) 
        totems |> Seq.nth (Random().Next(totems |> Seq.length))

    let getCardByExactName (name : string) =
        EntityJson.All
        |> Seq.find(fun e -> e.Name = name)
        |> parseEntityJsonToCard

    let getCardById (id : string) =
        EntityJson.All
        |> Seq.find(fun e -> e.Id = id)
        |> parseEntityJsonToCard

    let getCardIdsByNames (nameList : string list) =
        nameList |> List.map (fun name -> (getCardByExactName name).Id)

