module Arbitration.Infrastructure.MarketCache

open System
open Arbitration.Application.Interfaces
open Arbitration.Domain.Models.Assets
open Arbitration.Domain.Models.Prices
open Arbitration.Domain.Models.Spreads
open Microsoft.Extensions.Caching.Memory
open Microsoft.FSharp.Core

[<Literal>]
let private LastPricePrefix = "LastPrice"

[<Literal>]
let private LastSpreadPrefix = "LastSpread"

let getKey prefix key =
    prefix + "_" + key
    
let private  getCache (cache: IMemoryCache) prefix key =
    try
        let prefixKey = getKey prefix key
        match cache.TryGetValue<'t>(prefixKey) with
        | true, value -> value |> Ok
        | _ -> $"Key {prefixKey} not found" |> Error
    with  
    | ex ->
        $"Get cache error: {ex.Message}" |> Error
    
let private setCache (cache: IMemoryCache) expiration prefix key value =
    try
        let options = MemoryCacheEntryOptions(
            AbsoluteExpirationRelativeToNow = Nullable(expiration)
        )
        let id = cache.Set(getKey prefix key, value, options)
        id |> Ok
    with   
    | ex ->
        $"Set cache error: {ex.Message}" |> Error
        
let private removeCache (cache: IMemoryCache) prefix key =
    try        
        cache.Remove(getKey prefix key) |> Ok
    with   
    | ex ->
        $"Remove cache error: {ex.Message}" |> Error
    
let private lastPrice : Cache<AssetId, Price> = {    
    TryGet = fun env -> getCache env.Cache LastPricePrefix 
    Set = fun env price ->      
        (price.Asset, price)
        ||> setCache env.Cache env.Config.Cache.LastPriceExpiration LastPricePrefix
    Remove = fun env -> removeCache env.Cache LastPricePrefix
}

let private lastSpread : Cache<AssetSpreadId, Spread> = {    
    TryGet = fun env spreadAssetId ->
        getAssetSpreadKey spreadAssetId
        |> getCache env.Cache LastSpreadPrefix
    Set = fun env spread ->
        (spread |> getSpreadKey, spread)
        ||> setCache env.Cache env.Config.Cache.LastSpreadExpiration LastSpreadPrefix
    Remove = fun env spreadAssetId ->
        getAssetSpreadKey spreadAssetId
        |> removeCache env.Cache LastSpreadPrefix
}

let memoryMarketCache : MarketCache = {
    LastPrice = lastPrice
    LastSpread = lastSpread
}