module Arbitration.Application.Interfaces

open System
open System.Net.Http
open System.Threading.Tasks
open Arbitration.Domain.Models.Assets
open Arbitration.Domain.Models.Spreads
open Arbitration.Application.Configurations
open Arbitration.Domain.Types
open Binance.Net.Interfaces.Clients
open Microsoft.Extensions.Logging
open Npgsql

type Env = {   
    Postgres: PostgresEnv
    SpreadRepository: SpreadRepository
    SpreadApi: SpreadApi
    MarketData: MarketData
    BinanceRestClient: IBinanceRestClient 
    Logger: ILogger
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
    GetLastSpread: PostgresEnv -> AssetSpread -> Task<SpreadResult>
}

and MarketData = {
    GetPrice: Env -> AssetId -> Task<PriceResult>
    GetLastPrice: Env -> AssetId -> Task<PriceResult>
    GetLastSpread: Env -> AssetSpread -> Task<SpreadResult>
}

and Cache ={
    Set: string -> decimal -> unit
}

type ArbitrationQuery<'env, 'input, 'output> =
    'env -> 'input -> Task<'output>
    
type ArbitrationCommand<'env, 'state, 'input, 'output> =
    'env -> 'state -> 'input -> Task<'output * 'state>
    

    