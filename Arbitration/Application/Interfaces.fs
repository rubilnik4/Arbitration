module Arbitration.Application.Interfaces

open System
open System.Threading.Tasks
open Arbitration.Application.Configurations
open Arbitration.Domain.Models
open Arbitration.Domain.Types
open Npgsql

type PostgresEnv = {
    Source: NpgsqlDataSource
}

type MarketData = {
    GetPrice: AssetId -> Task<PriceResult>
    GetLastPrice: AssetId -> Task<PriceResult>
}

type SpreadRepository = {
    SaveSpread: PostgresEnv -> Spread -> Task<SpreadIdResult>
    GetLastPrice: PostgresEnv -> AssetId -> DateTime -> Task<PriceResult>
}

type Cache ={
    Set: string -> decimal -> unit
}

type Env = {   
    Postgres: PostgresEnv
    SpreadRepository: SpreadRepository
    MarketData: MarketData
    Logger: string -> unit
    Cache: Cache
    Config: Config
}

type Command<'env, 'state, 'input, 'output> =
    'env -> 'state -> 'input -> Task<'output * 'state>
    