module Arbitration.Infrastructure.MarketData

open Arbitration.Application.Interfaces

let private getPrice env asset =    
    env.Api.GetPrice env.Infra asset

let private getLastPrice env assetId = task {
    let marketRepository = env.Repository
    let lastPriceCache = env.Cache.LastPrice
    match lastPriceCache.TryGet env.Infra assetId with
    | Ok price ->
        return price |> Ok
    | Error _ ->
        match! marketRepository.GetLastPrice env.Infra assetId with
        | Ok price ->
            lastPriceCache.Set env.Infra price |> ignore
            return price |> Ok
        | Error error ->
            return error |> Error
    }
    
let private getLastSpread env spreadAssetId = task {
    let spreadRepository = env.Repository
    let lastSpreadCache = env.Cache.LastSpread
    match lastSpreadCache.TryGet env.Infra spreadAssetId with
    | Ok spread ->
        return spread |> Ok
    | Error _ ->
        match! spreadRepository.GetLastSpread env.Infra spreadAssetId with
        | Ok spread ->
            lastSpreadCache.Set env.Infra spread |> ignore
            return spread |> Ok
        | Error error ->
            return error |> Error
    }

let marketData : MarketData = {
    GetPrice = getPrice
    GetLastPrice = getLastPrice
    GetLastSpread = getLastSpread
}