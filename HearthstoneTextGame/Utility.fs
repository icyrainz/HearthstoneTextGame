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
        | value -> Path.Combine("Content", value)
        
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
        let allJson = JsonValue.Parse(File.ReadAllText(Utility.getContentPath "All.json"))
        JsonExtensions.AsArray(allJson)
        |> Array.map (fun jsonVal ->
            {
                Artist = JsonExtensions.TryGetProperty(jsonVal,"artist") |> Option.map (fun e -> JsonExtensions.AsString(e))
                Attack = JsonExtensions.TryGetProperty(jsonVal,"attack") |> Option.map (fun e -> JsonExtensions.AsInteger(e))
                Collectible = JsonExtensions.TryGetProperty(jsonVal,"collectible") |> Option.map (fun e -> JsonExtensions.AsBoolean(e))
                Cost = JsonExtensions.TryGetProperty(jsonVal,"cost") |> Option.map (fun e -> JsonExtensions.AsInteger(e))
                Durability = JsonExtensions.TryGetProperty(jsonVal,"durability") |> Option.map (fun e -> JsonExtensions.AsInteger(e))
                Elite = JsonExtensions.TryGetProperty(jsonVal,"elite") |> Option.map (fun e -> JsonExtensions.AsBoolean(e))
                Faction = JsonExtensions.TryGetProperty(jsonVal,"faction") |> Option.map (fun e -> JsonExtensions.AsString(e))
                Flavor = JsonExtensions.TryGetProperty(jsonVal,"flavor") |> Option.map (fun e -> JsonExtensions.AsString(e))
                Health = JsonExtensions.TryGetProperty(jsonVal,"health") |> Option.map (fun e -> JsonExtensions.AsInteger(e))
                HowToGet = JsonExtensions.TryGetProperty(jsonVal,"howtoget") |> Option.map (fun e -> JsonExtensions.AsString(e))
                HowToGetGold = JsonExtensions.TryGetProperty(jsonVal,"howtogetgold") |> Option.map (fun e -> JsonExtensions.AsString(e))
                Id = JsonExtensions.AsString(JsonExtensions.GetProperty(jsonVal,"id"))
                InPlayText = JsonExtensions.TryGetProperty(jsonVal,"inplaytext") |> Option.map (fun e -> JsonExtensions.AsString(e))
                Mechanics = JsonExtensions.AsArray(jsonVal) |> Array.map (fun e -> JsonExtensions.AsString(e)) |> Array.toList
                Name = JsonExtensions.AsString(JsonExtensions.GetProperty(jsonVal,"name"))
                PlayerClass = JsonExtensions.TryGetProperty(jsonVal,"playerclass") |> Option.map (fun e -> JsonExtensions.AsString(e))
                Race = JsonExtensions.TryGetProperty(jsonVal,"race") |> Option.map (fun e -> JsonExtensions.AsString(e))
                Rarity = JsonExtensions.TryGetProperty(jsonVal,"rarity") |> Option.map (fun e -> JsonExtensions.AsString(e))
                Text = JsonExtensions.TryGetProperty(jsonVal,"text") |> Option.map (fun e -> JsonExtensions.AsString(e))
                Type = JsonExtensions.AsString(JsonExtensions.GetProperty(jsonVal,"type"))
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