module Arbitration.Application.Commands.SpreadCommand

open System
open Arbitration.Application.Configurations
open Arbitration.Application.Environments.ApplicationTypes
open Arbitration.Domain.Models.Assets
open Arbitration.Domain.Models.Prices
open Arbitration.Domain.Models.Spreads
open Arbitration.Application.Interfaces
open Arbitration.Domain.DomainTypes
open Microsoft.Extensions.Logging

let private getSpread (priceA: Price) (priceB: Price) =    
    let spreadValue = (priceA.Value - priceB.Value) |> abs    
    {
        PriceA = priceA
        PriceB = priceB
        Value = spreadValue
        Time = DateTime.UtcNow
    }

let private getPrice env assetId = task {    
    match! env.Data.GetPrice env assetId with
    | Ok marketPrice ->
        env.Infra.Logger.LogInformation("Market price {assetId} get successfully", assetId)
        return marketPrice |> Ok
    | Error _ -> 
        match! env.Data.GetLastPrice env assetId with
        | Ok lastPrice ->
            env.Infra.Logger.LogInformation("Market price not found. Get last price {assetId}", assetId)
            return lastPrice |> Ok
        | Error _ ->
            env.Infra.Logger.LogError("No price available at all {assetId}", assetId)
            return NotFound $"No price available at all {assetId}" |> Error        
} 

let private updateState env spread state =
    let config = env.Config.Project
    let updatedHistory = spread.Value :: state.SpreadHistory |> List.take config.MaxHistorySize
    let isThresholdExceeded = spread.Value > config.SpreadThreshold
    if isThresholdExceeded then
        let assetSpreadId = getSpreadKey spread
        env.Logger.LogInformation("Threshold {threshold} is exceeded for spread {assetId}",
                                  config.SpreadThreshold, assetSpreadId)
    { state with
        LastSpread = Some spread.Value
        SpreadHistory = updatedHistory
        IsThresholdExceeded = isThresholdExceeded }    

let removeSpreadCache env spread =
    let assetSpreadId = getAssetSpreadId spread
    env.Cache.LastPrice.Remove env.Infra spread.PriceA.Asset |> ignore
    env.Cache.LastPrice.Remove env.Infra spread.PriceB.Asset |> ignore    
    env.Cache.LastSpread.Remove env.Infra assetSpreadId |> ignore
    
let private saveSpread env spread = task {
    let! spreadIdResult = env.Repository.SaveSpread env.Infra spread
    match spreadIdResult with
    | Ok spreadId ->
        removeSpreadCache env spread
        env.Infra.Logger.LogInformation("Spread {spreadId} saved successfully", spreadId)
        return spreadId |> Ok 
    | Error e -> 
        return e |> Error
}

let spreadCommand : Command<SpreadState, AssetSpreadId, SpreadResult> =
    fun env state input -> task {
        let assetSpreadId = normalizeSpreadAsset input
        env.Infra.Logger.LogInformation("Execute spread command for assets: {assetSpreadId}", assetSpreadId)
        
        let assetA, assetB = assetSpreadId
        let! priceAResult = assetA |> getPrice env 
        let! priceBResult = assetB |> getPrice env 

        match priceAResult, priceBResult with
        | Ok priceA, Ok priceB ->            
            let spread = getSpread priceA priceB
            match! saveSpread env spread with
            | Ok _ ->
                let newState = state |> updateState env.Infra spread 
                return spread |> Ok, newState
            | Error e ->
                return e |> Error, state            
        | Error e, _
        | _, Error e ->
            return e |> Error, state
    }