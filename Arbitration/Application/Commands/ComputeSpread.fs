module Arbitration.Application.Commands.ComputeSpread

open System.Threading.Tasks
open Arbitration.Application.Configurations
open Arbitration.Domain.Types
open Arbitration.Domain.Models
open Arbitration.Application.Interfaces
open Arbitration.Infrastructure

let private getSpread (priceA: Price) (priceB: Price) =    
    let spreadValue = (priceA.Value - priceB.Value) |> abs    
    {
        PriceA = priceA
        PriceB = priceB
        Value = spreadValue
        Time = TimerService.getUtcDatetime()
    }

let private getAvailablePrice marketData asset : Task<PriceResult> = task {
    let! marketPriceResult = marketData.GetPrice asset
    match marketPriceResult with
    | Ok marketPrice ->
        return marketPrice |> Ok
    | Error _ ->    
        let! lastPriceResult = marketData.GetLastPrice asset
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

let computeSpread : SpreadCommand =
    fun env state input -> task {
        let! priceAResult = input.AssetA |> getAvailablePrice env.MarketData 
        let! priceBResult = input.AssetB |> getAvailablePrice env.MarketData 

        match priceAResult, priceBResult with
        | Ok priceA, Ok priceB ->            
            let spread = getSpread priceA priceB
            let newState = state |> updateState env.Config.Project spread.Value 
            return spread |> Ok, newState
        | Error e, _
        | _, Error e ->
            return $"Error fetching prices: {e}, state {state}" |> Error, state
    }