namespace HearthstoneTextGame

module Hero =
    
    let heroPowers =
        EntityJson.All
        |> Seq.filter(fun e -> e.Type = "Hero Power")
        |> Seq.map(fun e ->
            { Id = e.Id
              Name = e.Name
              Cost = e.Cost.Value
              Text = e.Text.Value } : HeroPower)
        |> Seq.toList

    let playbleHeroes =
        EntityJson.All
        |> Seq.filter(fun e -> e.Collectible.IsSome 
                                && e.Collectible.Value
                                && e.Type = "Hero"
                                && e.PlayerClass.IsSome)
        |> Seq.map(fun e ->
            { Name = e.Name
              HeroClass = e.PlayerClass.Value } : Hero)
        |> Seq.toList

    let playableClasses =
        [ "Mage"
          "Druid"
          "Shaman"
          "Hunter"
          "Priest"
          "Rogue"
          "Warlock"
          "Paladin"
          "Warrior"
        ]

    let getHeroPower (hero : string) (original : bool) =
        let heroPowerName = 
            match (hero, original) with
            | "Mage", true -> "Fireblast"
            | "Druid", true -> "Shapeshift"
            | "Shaman", true -> "Totemic Call"
            | "Hunter", true -> "Steady Shot"
            | "Priest", true -> "Lesser Heal"
            | "Rogue", true -> "Dagger Mastery"
            | "Warlock", true -> "Life Tap"
            | "Paladin", true -> "Reinforce"
            | "Warrior", true -> "Armor Up!"
            | _, _ -> "Fireblast"
        heroPowers |> List.find(fun e -> e.Name = heroPowerName) 
    