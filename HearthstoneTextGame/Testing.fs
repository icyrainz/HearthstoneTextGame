module Testing

open FsUnit.Xunit
open Xunit
open HearthstoneTextGame

type ``With Reynad Zoodeck`` () =

    let zooDeck = Deck.PredefinedDecks |> List.find(fun deck -> deck.Name.Contains("zoo"))


    [<Fact>]
    member x.`` obvious 1 = 1 `` () =
        1 |> should equal 1

    [<Fact>]
    member x.``it should have at least 1 Doomguard`` () =
        zooDeck.CardIdList |> should contain (Card.getCardByExactName("Doomguard").Id)

type ``Using Hero Power`` () =
    
    let testGame = GameSession.Init()

    let register2Class class1 class2 =
        let p1, gameWithP1 = (Game.registerRandomDeckPlayerWithClass "testPlayer" class1 testGame).Value
        let p2, gameWithP1P2 = (Game.registerRandomDeckPlayerWithClass "testPlayer2" class2 gameWithP1).Value
        p1, p2, gameWithP1P2

    [<Fact>]
    member x.``Hunter should deal 2 damage`` () =
        let hunterPlayer, warlockPlayer, gameWithHunterAndWarlock = register2Class "Hunter" "Warlock"
        let gameWithHunterAndWarlockAfterHunterUseHeroPower = (Game.useHeroPower hunterPlayer None gameWithHunterAndWarlock).Value
        let hunterPlayerAfterUse = Game.getPlayer hunterPlayer.Guid gameWithHunterAndWarlockAfterHunterUseHeroPower
        let warlockPlayerAfterHit = Game.getPlayer warlockPlayer.Guid gameWithHunterAndWarlockAfterHunterUseHeroPower

        (snd hunterPlayerAfterUse.Value.HeroPower) |> should be True
        warlockPlayerAfterHit.Value.HeroCharacter.Hp |> should equal 28
