﻿namespace HearthstoneTextGame

[<AutoOpen>]
module Entity =

    type TargetHostile =
        | Friendly
        | Enemy
        | Any

    type TargetType =
        | AnyTarget of TargetHostile
        | MinionTarget of TargetHostile
        | FaceTarget of TargetHostile

    type Guid = 
        { value : string }

        override __.ToString() = __.value

        static member New() = { value = System.Guid.NewGuid().ToString() }

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

    type CardMechanic =
        | Battlecry
        | Taunt
        | ImmuneToSpellPower
        | Spellpower
        | OneTurnEffect
        | Charge
        | GrantCharge
        | AdjacentBuff
        | Aura
        | Freeze
        | Morph
        | HealTarget
        | Deathrattle
        | Combo
        | Divine_Shield
        | Windfury
        | Enrage
        | AffectedBySpellPower
        | Stealth
        | Secret
        | Silence
        | Poisonous
        | Summoned

    type Card =
        { Id : string
          Name : string
          Type : string
          Rarity : string option
          Race : string option
          CardClass : string option
          Cost : int option
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
              Cost = None
              Attack = None
              Health = None
              Durability = None
              Text = None
              Mechanics = [] }

    type CardOnHand =
        { Guid : Guid
          Cost : int
          Card : Card }
        static member Parse (card : Card) =
            { Guid = Guid.New()
              Cost = card.Cost.Value
              Card = card
            }


    type Deck =
        { Name : string
          DeckClass : string
          CardIdList : string list }

        member __.RemainingCardsCount = __.CardIdList.Length

        override __.ToString() = __.CardIdList |> List.fold (fun acc card -> acc + card + "\n") ""

        static member Empty =
            { Name = "EmptyDeck"
              DeckClass = "Other"
              CardIdList = [] }

    type Weapon =
        { Guid : Guid
          Card : Card
          Attack : int
          Durability : int
          Enchantments : string list }

        override __.ToString() = __.Card.ToString()
       
        static member Parse (card : Card) = 
            if card.Type <> "Weapon" then None
            else 
                Some { Guid = Guid.New()
                       Card = card
                       Enchantments = []
                       Attack = card.Attack.Value
                       Durability = card.Durability.Value 
                     }

    type Spell =
        { Card : Card }

        override __.ToString() = __.Card.ToString()
    
        static member Parse (card : Card) =
            if card.Type <> "Spell" then None
            else
                Some { Card = card }

    type ICharacter =
        abstract member Guid : Guid
        abstract member AttackValue : int
        abstract member SetAttack : int -> ICharacter
        abstract member Health : int
        abstract member SetHealth : int -> ICharacter
        abstract member GetDamage : int -> ICharacter
        abstract member GetHeal : int -> ICharacter
        abstract member CanAttack : bool
        abstract member DoAttack : ICharacter -> ICharacter * ICharacter

    type Face =
        { Guid : Guid
          Hp : int
          Armour : int
          AttackValue : int
          Weapon : Weapon option
          WeaponActivated : bool
          AttackTokens : int
          AttackCount : int
          HasImmunity : bool }

        override __.ToString() = __.Guid.value

        interface ICharacter with
            member __.Guid = __.Guid
            member __.AttackValue =
                if __.WeaponActivated && __.Weapon.IsSome then __.AttackValue + __.Weapon.Value.Attack
                else __.AttackValue
            member __.SetAttack (value) = { __ with AttackValue = value} :> ICharacter
            member __.Health = __.Hp
            member __.SetHealth (value) = { __ with Hp = value} :> ICharacter
            member __.GetDamage (value) =
                let mutable armour = __.Armour
                let mutable hp = __.Hp
                if not __.HasImmunity then
                    if value <= __.Armour then
                        armour <- __.Armour - value
                    else
                        hp <- __.Hp + __.Armour - value
                        armour <- 0
                        
                { __ with Armour = armour; Hp = hp } :> ICharacter
            member __.GetHeal(value) =
                let hp = __.Hp + System.Math.Min(value, Config.heroHp - __.Hp)
                { __ with Hp = hp } :> ICharacter
            member __.CanAttack = __.AttackCount < __.AttackTokens
            member __.DoAttack (target : ICharacter) =
                let mutable newMe = { __ with AttackCount = __.AttackCount + 1 } :> ICharacter
                let newWeapon = 
                    __.Weapon |> Option.bind(fun w ->
                        let newDura = w.Durability - 1
                        Some { w with Durability = newDura}
                    )
                newMe <- { (newMe :?> Face) with Weapon = newWeapon } :> ICharacter
                newMe <- newMe.GetDamage(target.AttackValue)
                let newTarget = target.GetDamage((__ :> ICharacter).AttackValue)
                newMe, newTarget
                

        static member Empty =
            { Guid = Guid.New()
              Hp = Config.heroHp
              Armour = 0
              AttackValue = 0
              Weapon = None
              WeaponActivated = false
              AttackTokens = 1
              AttackCount = 0
              HasImmunity = false }

    type Minion =
        { Guid : Guid
          Card : Card
          Enchantments : string list
          AttackValue : int
          AttackTokens : int
          AttackCount : int
          CurrentHealth : int
          MaxHealth : int
          HasDivineShield : bool
          HasTaunt : bool
          HasImmunity : bool }

        override __.ToString() = __.Guid.value + " " + __.Card.ToString()

        interface ICharacter with
            member __.Guid = __.Guid
            member __.AttackValue = __.AttackValue
            member __.SetAttack (value) = { __ with AttackValue = value } :> ICharacter 
            member __.Health = __.CurrentHealth
            member __.SetHealth (value) =
                let maxHealth = value
                let currentHealth = System.Math.Min(maxHealth, __.CurrentHealth)
                { __ with MaxHealth = maxHealth; CurrentHealth = currentHealth} :> ICharacter
            member __.GetDamage (damage) =
                let mutable hasDivineShield = __.HasDivineShield
                let mutable currentHealth = __.CurrentHealth
                do
                    if not __.HasImmunity then
                        if __.HasDivineShield then
                            hasDivineShield <- false
                        else
                            currentHealth <- __.CurrentHealth - damage
                { __ with HasDivineShield = hasDivineShield; CurrentHealth = currentHealth } :> ICharacter
            member __.GetHeal (amount) =
                let currentHealth =
                    __.CurrentHealth + System.Math.Min(amount, __.MaxHealth - __.CurrentHealth)
                { __ with CurrentHealth = currentHealth } :> ICharacter
            member __.CanAttack = __.AttackCount < __.AttackTokens
            member __.DoAttack (target : ICharacter) =
                let mutable newMe = { __ with AttackCount = __.AttackCount + 1 } :> ICharacter
                newMe <- newMe.GetDamage(target.AttackValue)
                let newTarget = target.GetDamage((__ :> ICharacter).AttackValue)
                newMe, newTarget

        static member Parse (card : Card) = 
            if card.Type <> "Minion" then None
            else
                let atkTokens =
                    match card.Mechanics |> List.exists(fun e -> e = "Windfury") with
                    | true -> 2
                    | false -> 1
                Some { Guid = Guid.New()
                       Card = card
                       Enchantments = []
                       AttackValue = card.Attack.Value
                       AttackTokens = atkTokens
                       AttackCount = 
                        match card.Mechanics |> List.exists(fun e -> e = "Charge") with
                        | true -> 0
                        | false -> atkTokens
                       CurrentHealth = card.Health.Value
                       MaxHealth = card.Health.Value
                       HasTaunt = card.Mechanics |> List.exists(fun e -> e = "Taunt")
                       HasDivineShield = card.Mechanics |> List.exists(fun e -> e = "Divine Shield")
                       HasImmunity = false }

    type Player =
        { Guid : Guid
          Name : string
          HeroClass : string
          HeroPower : HeroPower
          HeroPowerUsed : bool
          BonusSpellPower : int
          Deck : Deck
          Hand : CardOnHand list
          Face : Face
          Minions : Minion list
          ActiveSecret : Card option
          CurrentMana : int
          MaxMana : int 
        }

        override __.ToString() = __.Guid.value + " " + __.Name
    
        static member Empty =
          { Guid = Guid.New()
            Name = "EmptyPlayer"
            HeroClass = ""
            HeroPower = HeroPower.Empty
            HeroPowerUsed = false
            BonusSpellPower = 0
            Deck = Deck.Empty
            Hand = []
            Face = Face.Empty
            Minions = []
            ActiveSecret = None
            CurrentMana = 0
            MaxMana = 0 
          }
    
    type GamePhase =
        | NotStarted
        | Mulligan
        | Playing
        | EndGame

    type GameSession =
        { Guid : Guid
          Players : Player list
          StartPlayerGuid : Guid
          ActivePlayerGuid : Guid
          HasMulliganed : Guid list
          CurrentPhase : GamePhase
        }

        member __.PlayerCount = __.Players.Length

        override __.ToString() = __.Guid.value

        static member Init () =
            { Guid = Guid.New()
              Players = []
              StartPlayerGuid = { value = "" }
              ActivePlayerGuid = { value = "" }
              HasMulliganed = []
              CurrentPhase = GamePhase.NotStarted
            }