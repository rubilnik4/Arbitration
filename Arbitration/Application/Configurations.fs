module Arbitration.Application.Configurations

open Arbitration.Domain.Models

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

type Config = {
    Project: ProjectConfig
    Postgres: PostgresConfig    
}