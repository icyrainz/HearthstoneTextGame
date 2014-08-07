namespace HearthstoneTextGame

open System
open IntelliFactory.WebSharper

[<AutoOpen>]
module Entity =

    type TargetHostile =
        | Friendly
        | Enemy
        | Any

    type TargetType =
        | AnyTarget of TargetHostile
        | MinionTarget of TargetHostile

    type HeroPower =
        { Id : string
          Name : string
          Cost : int
          Text : string }

        member __.Target =
            match __.Name with
            | "Fireblast" ->
                Some <| AnyTarget Any
            | "Armor Up!" ->
                None
            | "Dagger Mastery" ->
                None
            | "Lesser Heal" ->
                Some <| AnyTarget Any
            | "Life Tap" ->
                None
            | "Reinforce" ->
                None
            | "Shapeshift" ->
                None
            | "Steady Shot" ->
                None
            | "Totemic Call" ->
                None
            | _ ->
                None

        override __.ToString() = __.Name

        static member Empty =
            { Id = ""
              Name = ""
              Cost = 0
              Text = "" }

    type Hero =
        { Name : string
          HeroClass : string }

        override __.ToString() = __.Name + " " + __.HeroClass

    type Card =
        { Id : string
          Name : string
          Type : string
          Rarity : string option
          Race : string option
          CardClass : string option
          Cost : int
          Attack : int option
          Health : int option
          Durability : int option
          Text : string option
          Mechanics : string list }

        override __.ToString() = __.Id + " " + __.Name

        static member Empty =
            { Id = ""
              Name = ""
              Type = "Other"
              Rarity = None
              Race = None
              CardClass = None
              Cost = 0
              Attack = None
              Health = None
              Durability = None
              Text = None
              Mechanics = [] }

    type Deck =
        { Name : string
          DeckClass : string
          CardIdList : string list }

        member __.RemainingCardsCount = __.CardIdList |> List.length

        override __.ToString() = __.CardIdList |> List.fold (fun acc card -> acc + card + "\n") ""

        static member Empty =
            { Name = "EmptyDeck"
              DeckClass = "Other"
              CardIdList = [] }

    type ICharacter =
        abstract member Guid : string
        abstract member Attack : int
        abstract member SetAttack : int -> ICharacter
        abstract member Health : int
        abstract member SetHealth : int -> ICharacter
        abstract member GetDamage : int -> ICharacter
        abstract member GetHeal : int -> ICharacter

    type HeroCharacter =
        { Guid : string
          Hp : int
          Armour : int
          AttackValue : int
          HasImmunity : bool }

        override __.ToString() = __.Guid

        interface ICharacter with
            member __.Guid = __.Guid
            member __.Attack = __.AttackValue
            member __.SetAttack(value) = { __ with AttackValue = value} :> ICharacter
            member __.Health = __.Hp
            member __.SetHealth(value) = { __ with Hp = value} :> ICharacter
            member __.GetDamage(value) =
                let mutable armour = 0
                let mutable hp = 0
                if not __.HasImmunity then
                    if value <= __.Armour then
                        armour <- __.Armour - value
                    else
                        armour <- 0
                        hp <- __.Hp - + __.Armour - value
                { __ with Armour = armour; Hp = hp } :> ICharacter
            member __.GetHeal(value) =
                let hp = __.Hp + Math.Min(value, Config.heroHp - __.Hp)
                { __ with Hp = hp } :> ICharacter

        static member Empty =
            { Guid = Guid.NewGuid().ToString()
              Hp = Config.heroHp
              Armour = 0
              AttackValue = 0
              HasImmunity = false }

    type Minion =
        { Guid : string
          Card : Card
          Enchantments : string list
          AttackValue : int
          CurrentHealth : int
          MaxHealth : int
          HasDivineShield : bool
          HasImmunity : bool }

        override __.ToString() = __.Guid + " " + __.Card.ToString()

        interface ICharacter with
            member __.Guid = __.Guid
            member __.Attack = __.AttackValue
            member __.SetAttack (value) = { __ with AttackValue = value } :> ICharacter 
            member __.Health = __.CurrentHealth
            member __.SetHealth (value) =
                let maxHealth = value
                let currentHealth = Math.Min(maxHealth, __.CurrentHealth)
                { __ with MaxHealth = maxHealth; CurrentHealth = currentHealth} :> ICharacter
            member __.GetDamage(damage) =
                let mutable hasDivineShield = __.HasDivineShield
                let mutable currentHealth = __.CurrentHealth
                do
                    if not __.HasImmunity then
                        if __.HasDivineShield then
                            hasDivineShield <- false
                        else
                            currentHealth <- __.CurrentHealth - damage
                { __ with HasDivineShield = hasDivineShield; CurrentHealth = currentHealth } :> ICharacter
            member __.GetHeal(amount) =
                let currentHealth =
                    __.CurrentHealth + Math.Min(amount, __.MaxHealth - __.CurrentHealth)
                { __ with CurrentHealth = currentHealth } :> ICharacter

        static member Parse (card : Card) = 
            if card.Type <> "Minion" then None
            else 
                Some { Guid = Guid.NewGuid().ToString()
                       Card = card
                       Enchantments = []
                       AttackValue = card.Attack.Value
                       CurrentHealth = card.Health.Value
                       MaxHealth = card.Health.Value
                       HasDivineShield = false
                       HasImmunity = false }

    type Weapon =
        { Card : Card
          Attack : int
          Durability : int
          Enchantments : string list }

        override __.ToString() = __.Card.ToString()
       
        static member Parse (card : Card) = 
            if card.Type <> "Weapon" then None
            else 
                Some { Card = card
                       Enchantments = []
                       Attack = card.Attack.Value
                       Durability = card.Durability.Value }

    type Spell =
        { Card : Card }

        override __.ToString() = __.Card.ToString()
    
        static member Parse (card : Card) =
            if card.Type <> "Spell" then None
            else
                Some { Card = card }

    type Player =
        { Guid : string
          Name : string
          HeroClass : string
          HeroPower : HeroPower * bool
          BonusSpellPower : int
          Deck : Deck
          Hand : string list
          HeroCharacter : HeroCharacter
          MinionPosition : Minion list
          ActiveWeapon : Weapon option
          ActiveSecret : Card option
          CurrentMana : int
          MaxMana : int }

        override __.ToString() = __.Guid + " " + __.Name
    
        static member Empty =
          { Guid = Guid.NewGuid().ToString()
            Name = "EmptyPlayer"
            HeroClass = ""
            HeroPower = (HeroPower.Empty, false)
            BonusSpellPower = 0
            Deck = Deck.Empty
            Hand = []
            HeroCharacter = HeroCharacter.Empty
            MinionPosition = []
            ActiveWeapon = None
            ActiveSecret = None
            CurrentMana = 0
            MaxMana = 0 }

    type GameSession =
        { Guid : string
          Players : Player list }

        override __.ToString() = __.Guid

        static member Init () =
            { Guid = Guid.NewGuid().ToString()
              Players = [] }