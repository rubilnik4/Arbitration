module Arbitration.Application.Commands.SpreadCommand

open Arbitration.Application.Commands.Commands
open Arbitration.Application.Configurations
open Arbitration.Domain.Models.Assets
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

let private getPrice env assetId = task {
    let! marketPriceResult = env.MarketData.GetPrice env assetId
    match marketPriceResult with
    | Ok marketPrice ->
        return marketPrice |> Ok
    | Error _ ->    
        let! lastPriceResult = env.MarketData.GetLastPrice env assetId
        match lastPriceResult with
        | Ok lastPrice ->                
            return lastPrice |> Ok
        | Error _ ->
            return $"No data available at all for {assetId}" |> Error        
} 

let private updateState config spread state =    
    let updatedHistory = spread.Value :: state.SpreadHistory |> List.take config.MaxHistorySize
    let isThresholdExceeded = spread.Value > config.SpreadThreshold
    { state with
        LastSpread = Some spread.Value
        SpreadHistory = updatedHistory
        IsThresholdExceeded = isThresholdExceeded }

let removeSpreadCache env spread =
    let assetSpreadId = getAssetSpreadId spread
    env.MarketCache.LastPrice.Remove env spread.PriceA.Asset |> ignore
    env.MarketCache.LastPrice.Remove env spread.PriceB.Asset |> ignore    
    env.MarketCache.LastSpread.Remove env assetSpreadId |> ignore
    
let private saveSpread env spread = task {
    let! spreadIdResult =  env.MarketRepository.SaveSpread env.Postgres spread
    match spreadIdResult with
    | Ok spreadId ->
        removeSpreadCache env spread
        return Ok spreadId
    | Error e -> 
        return Error e
}

let spreadCommand : SpreadCommand =
    fun env state input -> task {
        let assetA, assetB = normalizeSpreadAsset input
        let! priceAResult = assetA |> getPrice env 
        let! priceBResult = assetB |> getPrice env 

        match priceAResult, priceBResult with
        | Ok priceA, Ok priceB ->            
            let spread = getSpread priceA priceB
            match! saveSpread env spread with
            | Ok _ ->
                let newState = state |> updateState env.Config.Project spread 
                return spread |> Ok, newState
            | Error e ->
                return e |> Error, state            
        | Error e, _
        | _, Error e ->
            return $"Error fetching prices: {e}, state {state}" |> Error, state
    }