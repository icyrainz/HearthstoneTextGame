module Testing

open FsUnit.Xunit
open Xunit
open HearthstoneTextGame

type ``With Reynad Zoodeck`` () =

    let zooDeck = Deck.PredefinedDecks |> List.find(fun deck -> deck.Name.Contains("zoo"))

    [<Fact>]
    member x.``it should have at least 1 Doomguard`` () =
        zooDeck.CardIdList |> should contain (Card.getCardByExactName("Doomguard").Id)

type ``Using Hero Power`` () =
    
    let testGame = GameSession.Init()

    let register2Class class1 class2 =
        let p1, gameWithP1 = (Game.registerRandomDeckPlayerWithClass "testPlayer" class1 testGame).Value
        let p1withMana = {p1 with MaxMana = 2; CurrentMana = 2}
        let gameWithP1withMana = Game.updatePlayerToGame p1withMana gameWithP1
        let p2, gameWithP1P2 = (Game.registerRandomDeckPlayerWithClass "testPlayer2" class2 gameWithP1withMana).Value
        p1withMana, p2, gameWithP1P2

    [<Fact>]
    member x.``Hunter should deal 2 damage`` () =
        let hunterPlayer, warlockPlayer, gameWithHunterAndWarlock = register2Class "Hunter" "Warlock"
        let gameWithHunterAndWarlockAfterHunterUseHeroPower = (Game.useHeroPower hunterPlayer.Guid None gameWithHunterAndWarlock).Value
        let hunterPlayerAfterUse = Game.getPlayer hunterPlayer.Guid gameWithHunterAndWarlockAfterHunterUseHeroPower
        let warlockPlayerAfterHit = Game.getPlayer warlockPlayer.Guid gameWithHunterAndWarlockAfterHunterUseHeroPower

        hunterPlayerAfterUse.Value.HeroPowerUsed |> should be True
        hunterPlayerAfterUse.Value.CurrentMana |> should equal 0
        warlockPlayerAfterHit.Value.Face.Hp |> should equal 28

    [<Fact>]
    member x.``Rogue should have the Knife`` () =
        let roguePlayer, _, game = register2Class "Rogue" "Warlock"
        let gameAfter = (Game.useHeroPower roguePlayer.Guid None game).Value
        let rogueAfterUse = (Game.getPlayer roguePlayer.Guid gameAfter).Value

        rogueAfterUse.CurrentMana |> should equal 0
        rogueAfterUse.HeroPowerUsed |> should be True
        rogueAfterUse.Face.Weapon.IsSome |> should be True
        rogueAfterUse.Face.Weapon.Value.Card.Name |> should equal "Wicked Knife"

    [<Fact>]
    member x.``Shaman should summon a totem`` () =
        let shaman, _, game = register2Class "Shaman" "Warlock"
        let gameAfter = (Game.useHeroPower shaman.Guid None game).Value
        let shamanAfterUse = (Game.getPlayer shaman.Guid gameAfter).Value
        
        shamanAfterUse.CurrentMana |> should equal 0
        shamanAfterUse.Minions.Length |> should equal 1
        shamanAfterUse.Minions.Head.Card.Race.Value |> should equal "Totem"

    [<Fact>]
    member x.``Paladin should have a 1/1`` () =
        let paladin, _, game = register2Class "Paladin" "Warlock"
        let gameAfter = (Game.useHeroPower paladin.Guid None game).Value
        let paladinAfterUse = (Game.getPlayer paladin.Guid gameAfter).Value

        paladinAfterUse.CurrentMana |> should equal 0
        paladinAfterUse.Minions.Head.Card.Attack.Value |> should equal 1
        paladinAfterUse.Minions.Head.Card.Health.Value |> should equal 1
    
    [<Fact>]
    member x.``Warlock should draw card and' lose health`` () =
        let player, _, game = register2Class "Warlock" "Warlock"
        let gameAfter = (Game.useHeroPower player.Guid None game).Value
        let playerAfterUse = (Game.getPlayer player.Guid gameAfter).Value

        playerAfterUse.CurrentMana |> should equal 0
        playerAfterUse.Face.Hp |> should equal (Config.heroHp - 2)
        playerAfterUse.Hand.Length |> should equal 1
        playerAfterUse.Deck.RemainingCardsCount |> should equal (Config.maxDeckSize - 1)