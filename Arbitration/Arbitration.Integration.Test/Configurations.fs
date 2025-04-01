module Arbitration.Integration.Test.Configurations

open System
open Arbitration.Application.Configurations

let getAssetConfig() = {
    AssetA = "BTCUSDT_QUARTER"
    AssetB = "BTCUSDT_BI-QUARTER"
}

let getProjectConfig() = {
    Assets = getAssetConfig()
    SpreadThreshold = 100000m
    MaxHistorySize = 5
}

let getCacheConfig() = {  
    LastPriceExpiration = TimeSpan.FromMinutes(1L)
    LastSpreadExpiration = TimeSpan.FromMinutes(1L)
}

let getConfig connectionString = {
    Project = getProjectConfig()
    Postgres = { ConnectionString = connectionString }
    Cache = getCacheConfig()
}