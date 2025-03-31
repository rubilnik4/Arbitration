module Arbitration.Infrastructure.MarketData

open Arbitration.Application.Interfaces

let private getPrice env asset =
    let marketApi = env.MarketApi
    marketApi.GetPrice env asset

let private getLastPrice env assetId = task {
    let marketRepository = env.MarketRepository
    let lastPriceCache = env.MarketCache.LastPrice
    match lastPriceCache.TryGet env assetId with
    | Ok price ->
        return price |> Ok
    | Error _ ->
        match! marketRepository.GetLastPrice env assetId with
        | Ok price ->
            lastPriceCache.Set env price |> ignore
            return price |> Ok
        | Error error ->
            return error |> Error
    }
    
let private getLastSpread env spreadAssetId = task {
    let spreadRepository = env.MarketRepository
    let lastSpreadCache = env.MarketCache.LastSpread
    match lastSpreadCache.TryGet env spreadAssetId with
    | Ok spread ->
        return spread |> Ok
    | Error _ ->
        match! spreadRepository.GetLastSpread env spreadAssetId with
        | Ok spread ->
            lastSpreadCache.Set env spread |> ignore
            return spread |> Ok
        | Error error ->
            return error |> Error
    }

let marketData : MarketData = {
    GetPrice = getPrice
    GetLastPrice = getLastPrice
    GetLastSpread = getLastSpread
}