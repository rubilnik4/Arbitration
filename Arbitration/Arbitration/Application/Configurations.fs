module Arbitration.Application.Configurations

open System
open Arbitration.Domain.Models.Assets

type AssetConfig = {
    AssetA: AssetId
    AssetB: AssetId
}

type ProjectConfig = {
    Assets: AssetConfig
    SpreadThreshold: decimal
    MaxHistorySize: int
}

type PostgresConfig = {
    ConnectionString: string
}

type CacheConfig = {  
    LastPriceExpiration: TimeSpan
    LastSpreadExpiration: TimeSpan
}

type Config = {
    Project: ProjectConfig
    Postgres: PostgresConfig
    Cache: CacheConfig
}