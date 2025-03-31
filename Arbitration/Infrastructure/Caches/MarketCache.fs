module Arbitration.Infrastructure.MarketCache

open System
open Arbitration.Application.Interfaces
open Arbitration.Domain.Models.Assets
open Arbitration.Domain.Models.Prices
open Arbitration.Domain.Models.Spreads
open Arbitration.Domain.DomainTypes
open Arbitration.Infrastructure.Caches.MarketCacheKeys
open Microsoft.Extensions.Caching.Memory
open Microsoft.Extensions.Logging
open Microsoft.FSharp.Core
    
let private getCache env prefix key =
    let prefixKey = getKey prefix key
    try        
        env.Logger.LogDebug("Getting cache for key {key}", prefixKey)
        
        match env.Cache.TryGetValue<'t>(prefixKey) with
        | true, value ->
            env.Logger.LogDebug("Cache for key {key} get successfully", prefixKey)
            value |> Ok
        | _ ->
            env.Logger.LogDebug("Cache for key {key} not found", prefixKey)
            NotFound $"Key {prefixKey} not found" |> Error
    with  
    | ex ->
        env.Logger.LogError(ex, "Failed get cache for key {key}", prefixKey)
        CacheError $"Failed get cache for key {prefixKey}" |> Error
    
let inline private setCache env expiration prefix key value =
    let prefixKey = getKey prefix key
    try
        env.Logger.LogDebug("Setting cache for key {key}", prefixKey)
        let options = MemoryCacheEntryOptions(
            AbsoluteExpirationRelativeToNow = Nullable(expiration)
        )
        let id = env.Cache.Set(prefixKey, value, options)
        env.Logger.LogDebug("Cache for key {key} set successfully", prefixKey)
        id |> Ok
    with   
    | ex ->
        env.Logger.LogError(ex, "Failed set cache for key {key}", prefixKey)
        CacheError $"Failed set cache for key {prefixKey}" |> Error
        
let private removeCache env prefix key =
    let prefixKey = getKey prefix key
    try
        env.Logger.LogDebug("Removing cache for key {key}", prefixKey)
        env.Cache.Remove(prefixKey) 
        env.Logger.LogDebug("Cache for key {key} Removing successfully", prefixKey)
        () |> Ok
    with   
    | ex ->
        env.Logger.LogError(ex, "Failed remove cache for key {key}", prefixKey)
        CacheError $"Failed remove cache for key {prefixKey}" |> Error
    
let private lastPrice : Cache<AssetId, Price> = {    
    TryGet = fun env -> getCache env LastPricePrefix 
    Set = fun env price ->      
        (price.Asset, price)
        ||> setCache env env.Config.Cache.LastPriceExpiration LastPricePrefix
    Remove = fun env -> removeCache env LastPricePrefix
}

let private lastSpread : Cache<AssetSpreadId, Spread> = {    
    TryGet = fun env spreadAssetId ->
        getAssetSpreadKey spreadAssetId
        |> getCache env LastSpreadPrefix
    Set = fun env spread ->
        (spread |> getSpreadKey, spread)
        ||> setCache env env.Config.Cache.LastSpreadExpiration LastSpreadPrefix
    Remove = fun env spreadAssetId ->
        getAssetSpreadKey spreadAssetId
        |> removeCache env LastSpreadPrefix
}

let memoryMarketCache : MarketCache = {
    LastPrice = lastPrice
    LastSpread = lastSpread
}