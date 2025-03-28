module Arbitration.Application.Environments

open Arbitration.Application.Configurations
open Arbitration.Application.Interfaces
open Npgsql

type PostgresEnv = {
    Source: NpgsqlDataSource
}

type Env = {   
    Postgres: PostgresEnv
    SpreadRepository: SpreadRepository
    MarketData: MarketData
    Logger: string -> unit
    Cache: Cache
    Config: Config
}