module Arbitration.Application.Configurations

open System
open System.ComponentModel.DataAnnotations
open Arbitration.Domain.Models.Assets

[<CLIMutable>]
type AssetConfig = {
    [<Required>]
    AssetA: string
    
    [<Required>]
    AssetB: string
}

[<CLIMutable>]
type ProjectConfig = {
    [<Required>]
    [<Range(1, 50)>]
    MaxHistorySize: int
    
    [<Required>]
    [<Range(0.0, Double.MaxValue)>]
    SpreadThreshold: decimal
    
    [<Required>]
    Assets: AssetConfig
}

[<CLIMutable>]
type PostgresConfig = {
    [<Required>]
    ConnectionString: string
}

[<CLIMutable>]
type CacheConfig = {
    [<Range(typeof<TimeSpan>, "00:00:00", "01:00:00")>]
    LastPriceExpiration: TimeSpan
    
    [<Range(typeof<TimeSpan>, "00:00:00", "01:00:00")>]
    LastSpreadExpiration: TimeSpan
}

[<CLIMutable>]
type AppConfig = {
    [<Required>]
    Project: ProjectConfig
    
    [<Required>]
    Postgres: PostgresConfig
    
    [<Required>]
    Cache: CacheConfig
}