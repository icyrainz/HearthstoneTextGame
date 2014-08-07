namespace HearthstoneTextGame

open System
open System.Web.Hosting
open System.IO
open FSharp.Data
open IntelliFactory.WebSharper

module Utility =
    let rec insert v i l = 
        match i, l with
        | 0, xs -> v :: xs
        | i, x :: xs -> x :: insert v (i - 1) xs
        | i, [] -> failwith "index out of range"
    
    let rec remove i l = 
        match i, l with
        | 0, x :: xs -> xs
        | i, x :: xs -> x :: remove (i - 1) xs
        | i, [] -> failwith "index out of range"
    
    let removeRandomElem lst = 
        let rng = Random().Next(lst |> List.length)
        let item = List.nth lst rng
        let newLst = remove rng lst
        item, newLst

    let getContentPath path = 
        match System.Web.Hosting.HostingEnvironment.MapPath("~/Content/" + path) with
        | null -> path
        | value -> value
        
    let predefinedDecksFileName = 
        [ "reynad_zoo.Warlock.deck"]
        |> List.map (fun deck -> getContentPath deck)         

[<AutoOpen>]
module EntityJson =

    type T =
        { Artist : string option
          Attack : int option
          Collectible : bool option
          Cost : int option
          Durability : int option
          Elite : bool option
          Faction : string option
          Flavor : string option
          Health : int option
          HowToGet : string option
          HowToGetGold : string option
          Id : string
          InPlayText : string option
          Mechanics : string list
          Name : string
          PlayerClass : string option
          Race : string option
          Rarity : string option
          Text : string option
          Type : string }

    let All =         
        JsonValue.Parse(File.ReadAllText(Utility.getContentPath "All.json")).AsArray()
        |> Array.map (fun jsonVal ->
            {
                Artist = jsonVal.TryGetProperty("artist") |> Option.map (fun e -> e.AsString())
                Attack = jsonVal.TryGetProperty("attack") |> Option.map (fun e -> e.AsInteger())
                Collectible = jsonVal.TryGetProperty("collectible") |> Option.map (fun e -> e.AsBoolean())
                Cost = jsonVal.TryGetProperty("cost") |> Option.map (fun e -> e.AsInteger())
                Durability = jsonVal.TryGetProperty("durability") |> Option.map (fun e -> e.AsInteger())
                Elite = jsonVal.TryGetProperty("elite") |> Option.map (fun e -> e.AsBoolean())
                Faction = jsonVal.TryGetProperty("faction") |> Option.map (fun e -> e.AsString())
                Flavor = jsonVal.TryGetProperty("flavor") |> Option.map (fun e -> e.AsString())
                Health = jsonVal.TryGetProperty("health") |> Option.map (fun e -> e.AsInteger())
                HowToGet = jsonVal.TryGetProperty("howtoget") |> Option.map (fun e -> e.AsString())
                HowToGetGold = jsonVal.TryGetProperty("howtogetgold") |> Option.map (fun e -> e.AsString())
                Id = jsonVal.GetProperty("id").AsString()
                InPlayText = jsonVal.TryGetProperty("inplaytext") |> Option.map (fun e -> e.AsString())
                Mechanics = jsonVal.AsArray() |> Array.map (fun e -> e.AsString()) |> Array.toList
                Name = jsonVal.GetProperty("name").AsString()
                PlayerClass = jsonVal.TryGetProperty("playerclass") |> Option.map (fun e -> e.AsString())
                Race = jsonVal.TryGetProperty("race") |> Option.map (fun e -> e.AsString())
                Rarity = jsonVal.TryGetProperty("rarity") |> Option.map (fun e -> e.AsString())
                Text = jsonVal.TryGetProperty("text") |> Option.map (fun e -> e.AsString())
                Type = jsonVal.GetProperty("type").AsString()
            }
        )
        |> Array.toList

[<JavaScript>]
module Config =
    let maxNumberOfPlayers = 2
    let maxDeckSize = 30
    let heroHp = 30
    let maxMana = 10
     

//type HeroClass =
//    | Mage
//    | Hunter
//    | Priest
//    | Warlock
//    | Paladin
//    | Warrior
//    | Rogue
//    | Shaman
//    | Druid
//    | Other
//
//type CardType =
//    | Minion
//    | Spell
//    | Weapon
//    | Other
//
//type CardRarity =
//    | Legendary
//    | Epic
//    | Rare
//    | Common
//    | Free
//
//type CardRace =
//    | Beast
//    | Demon
//    | Pirate
//    | Dragon
//    | Murloc
//    | Totem
//
//type CardMechanic =
//    | BattleCry
//    | Unknown