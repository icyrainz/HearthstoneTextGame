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