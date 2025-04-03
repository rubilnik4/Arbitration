module Arbitration.Integration.Test.Configurations

open System
open Arbitration.Application.ProjectConfig

let getAssetConfig() = {
    AssetA = "BTCUSDT_250627"
    AssetB = "BTCUSDT_250926"
}

let getProjectConfig() = {
    Assets = getAssetConfig()
    SpreadThreshold = 100000m
    MaxHistorySize = 5
    AssetLoadingDelay = TimeSpan.FromSeconds(15L)
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