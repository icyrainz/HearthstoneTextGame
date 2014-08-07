namespace HearthstoneTextGame

open System
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
        
    let predefinedDecksFileName = 
        [ "reynad_zoo.Warlock.deck"]
        |> List.map (fun deck -> Path.Combine("Extra",deck))         

[<AutoOpen>]
module EntityJson =
    
    type T = JsonProvider<"Extra\All.json">.Root
    let All = JsonProvider<"Extra\All.json">.GetSamples()

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