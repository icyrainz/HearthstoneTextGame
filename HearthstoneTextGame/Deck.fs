namespace HearthstoneTextGame

open System
open System.IO
open System.Text.RegularExpressions

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

    let parseDeckInCockatrice (text : string) =
        try
            let pattern = @"(\d{1}) (.+)"
            let lines = text.Split([|Environment.NewLine|], StringSplitOptions.None)
            [ for line in lines do
                let captureGroup = Regex.Match(line.Trim(), pattern).Groups
                let numCard = captureGroup.Item(1).Value |> int
                let cardName = captureGroup.Item(2).Value
                for i = 1 to numCard do yield cardName
            ] |> Card.getCardIdsByNames
        with
            _ -> failwith "Cannot parse deck"

    let PredefinedDecks = 
        Utility.predefinedDecksFileName |> List.map(fun deckFileName ->
            let deckInfo = deckFileName.Split([|'.'|])
            let deckName = deckInfo.[0]
            let deckClass = deckInfo.[1]
            let deckCardList = parseDeckInCockatrice <| File.ReadAllText(deckFileName)
            { Name = deckName
              DeckClass = deckClass
              CardIdList = deckCardList
            }
                
        )