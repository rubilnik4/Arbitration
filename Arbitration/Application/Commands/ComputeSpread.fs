module Arbitration.Application.Commands.ComputeSpread

open System
open FSharpPlus
open FSharpPlus.Data
open Arbitration.Domain.Models
open Arbitration.Application.Interfaces

let private getSpread (assetA: Asset) (assetB: Asset) =    
    let spreadValue = (assetA.Value - assetB.Value) |> abs
    let now = DateTimeOffset.UtcNow
    {
        AssetA = assetA.Asset
        AssetB = assetB.Asset
        Value = spreadValue
        Time = now
    }

let private getAvailablePrice marketData asset : SpreadResultT<Asset> = monad {    
    let! marketPrice = marketData.GetPrice asset
    
    match marketPrice with
    | Ok price ->
        return price |> Ok
    | Error _ ->    
        let! lastPrice = marketData.GetLastPrice asset
        match lastPrice with
        | Ok price ->                
            return price |> Ok
        | Error _ ->
            return $"No data available at all for {asset}" |> Error            
}

let updateState (spread: decimal) (threshold: decimal) : SpreadState -> SpreadState =
    fun state ->
        let updatedHistory = spread :: state.SpreadHistory |> List.take 10
        let isThresholdExceeded = spread > threshold
        { state with
            LastSpread = Some spread
            SpreadHistory = updatedHistory
            IsThresholdExceeded = isThresholdExceeded }
    
let handle assetA assetB : SpreadAppT<Spread> = monad {
    let! env = ask
    
    let! assetA = assetA |> (getAvailablePrice env.MarketData) |> StateT.lift |> ReaderT.lift
    let! assetB = assetB |> (getAvailablePrice env.MarketData) |> StateT.lift |> ReaderT.lift   
   
    let spread = getSpread assetA assetB
    // let spread assetA assetB : SpreadResult<Spread> = monad {
    //     let! assetA = assetA
    //     let! assetB = assetB
    //     return getSpread assetA assetB
    // }
    return spread
}


    // let! state = get
    //     // let newState = { state with IsThresholdExceeded = true }
    //     // do! put newState