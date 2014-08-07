namespace HearthstoneTextGame

open System

module Deck =

    let drawCardFromDeck (deck : Deck) = 
        let drawCard, remainCardList = deck.CardIdList |> Utility.removeRandomElem
        drawCard, { deck with CardIdList = remainCardList }

    let isDeckValid (deck : Deck) =
        let cardList = deck.CardIdList |> Seq.map(fun e -> Card.getCardById(e))
        cardList |> Seq.groupBy (fun e -> e.Id, e.Rarity)
        |> Seq.exists(fun ((id, rarity), cards) ->
            (rarity.IsSome && rarity.Value = "Legendary" && (cards |> Seq.length) > 1)
            || (cards |> Seq.length) > 2 ) |> not

    let getRandomDeck (hero : string) =
        let desc = 
            { Name = "RandomDeck"
              DeckClass = hero
              CardIdList = [] }
        let rngDeck = ref <| desc
        [1 .. Config.maxDeckSize]
        |> List.iter(fun _ ->
            let tempDeck = ref <| {desc with CardIdList = Card.getRandomPlayableCard(hero).Id :: (!rngDeck).CardIdList}
            while (not <| isDeckValid(!tempDeck)) do
                tempDeck := {desc with CardIdList = Card.getRandomPlayableCard(hero).Id :: (!rngDeck).CardIdList}
            rngDeck := !tempDeck
            )

        !rngDeck