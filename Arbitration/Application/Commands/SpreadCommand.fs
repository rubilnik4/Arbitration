module Arbitration.Application.Commands.SpreadCommand

open System.Threading.Tasks
open Arbitration.Application.Commands.Commands
open Arbitration.Application.Configurations
open Arbitration.Domain.Models.Assets
open Arbitration.Domain.Types
open Arbitration.Domain.Models.Prices
open Arbitration.Domain.Models.Spreads
open Arbitration.Application.Interfaces
open Arbitration.Infrastructure

let private getSpread (priceA: Price) (priceB: Price) =    
    let spreadValue = (priceA.Value - priceB.Value) |> abs    
    {
        PriceA = priceA
        PriceB = priceB
        Value = spreadValue
        Time = DateTimeUtils.getUtcDatetime()
    }

let private getPrice env asset : Task<PriceResult> = task {
    let! marketPriceResult = env.MarketData.GetPrice env asset
    match marketPriceResult with
    | Ok marketPrice ->
        return marketPrice |> Ok
    | Error _ ->    
        let! lastPriceResult = env.MarketData.GetLastPrice env asset
        match lastPriceResult with
        | Ok lastPrice ->                
            return lastPrice |> Ok
        | Error _ ->
            return $"No data available at all for {asset}" |> Error        
} 

let private updateState (config: ProjectConfig) spread state =    
    let updatedHistory = spread :: state.SpreadHistory |> List.take config.MaxHistorySize
    let isThresholdExceeded = spread > config.SpreadThreshold
    { state with
        LastSpread = Some spread
        SpreadHistory = updatedHistory
        IsThresholdExceeded = isThresholdExceeded }   

let spreadCommand : SpreadCommand =
    fun env state input -> task {
        let spreadAssets = normalizeAsset input
        let! priceAResult = spreadAssets.AssetA |> getPrice env 
        let! priceBResult = spreadAssets.AssetB |> getPrice env 

        match priceAResult, priceBResult with
        | Ok priceA, Ok priceB ->            
            let spread = getSpread priceA priceB
            let newState = state |> updateState env.Config.Project spread.Value 
            return spread |> Ok, newState
        | Error e, _
        | _, Error e ->
            return $"Error fetching prices: {e}, state {state}" |> Error, state
    }