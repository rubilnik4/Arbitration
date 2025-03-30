module Arbitration.Application.Interfaces

open System
open System.Net.Http
open System.Threading.Tasks
open Arbitration.Application.Configurations
open Arbitration.Domain.Models
open Arbitration.Domain.Types
open Microsoft.Extensions.Logging
open Npgsql

type Env = {   
    Postgres: PostgresEnv
    SpreadRepository: SpreadRepository
    SpreadApi: SpreadApi
    MarketData: MarketData
    HttpClientFactory: IHttpClientFactory
    Logger: ILoggerFactory
    Cache: Cache
    Config: Config
}

and PostgresEnv = {
    Source: NpgsqlDataSource
}

and SpreadApi = {
    GetPrice: Env -> AssetId -> Task<PriceResult>
}

and SpreadRepository = {
    SaveSpread: PostgresEnv -> Spread -> Task<SpreadIdResult>
    GetLastPrice: PostgresEnv -> AssetId -> Task<PriceResult>
    GetLastSpread: PostgresEnv -> SpreadAsset -> Task<SpreadResult>
}

and MarketData = {
    GetPrice: Env -> AssetId -> Task<PriceResult>
    GetLastPrice: Env -> AssetId -> Task<PriceResult>
    GetLastSpread: Env -> SpreadAsset -> Task<SpreadResult>
}

and Cache ={
    Set: string -> decimal -> unit
}

type ArbitrationQuery<'env, 'input, 'output> =
    'env -> 'input -> Task<'output>
    
type ArbitrationCommand<'env, 'state, 'input, 'output> =
    'env -> 'state -> 'input -> Task<'output * 'state>
    
let createEnv () : Env =
    let config = { 
        ConnectionString = "your_connection_string"
        ApiKey = "your_api_key"
        ProjectThreshold = 0.1m
        ApiUrl = "https://api.example.com"
    }
    { Config = config }
    