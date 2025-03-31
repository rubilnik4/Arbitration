module Arbitration.Application.Interfaces

open System
open System.Net.Http
open System.Threading.Tasks
open Arbitration.Domain.Models.Assets
open Arbitration.Domain.Models.Prices
open Arbitration.Domain.Models.Spreads
open Arbitration.Application.Configurations
open Arbitration.Domain.Types
open Binance.Net.Interfaces.Clients
open Microsoft.Extensions.Caching.Memory
open Microsoft.Extensions.Logging
open Npgsql

type Env = {   
    Postgres: Postgres
    MarketRepository: MarketRepository
    MarketApi: MarketApi
    MarketData: MarketData
    MarketCache: MarketCache
    BinanceRestClient: IBinanceRestClient
    Cache: IMemoryCache
    Logger: ILogger    
    Config: Config
}

and Postgres = {
    Source: NpgsqlDataSource
}

and MarketApi = {
    GetPrice: Env -> AssetId -> Task<PriceResult>
}

and MarketRepository = {
    SaveSpread: Postgres -> Spread -> Task<SpreadIdResult>
    GetLastPrice: Postgres -> AssetId -> Task<PriceResult>
    GetLastSpread: Postgres -> AssetSpreadId -> Task<SpreadResult>
}

and MarketData = {
    GetPrice: Env -> AssetId -> Task<PriceResult>
    GetLastPrice: Env -> AssetId -> Task<PriceResult>
    GetLastSpread: Env -> AssetSpreadId -> Task<SpreadResult>
}

and MarketCache = {
    LastPrice : Cache<AssetId, Price>
    LastSpread : Cache<AssetSpreadId, Spread>
}

and Cache<'TKey, 'T> = {
    TryGet: Env -> 'TKey -> MarketResult<'T>
    Set: Env -> 'T -> MarketResult<'T>
    Remove: Env -> 'TKey -> MarketResult<unit>
}
   
type Query<'input, 'output> =
    Env -> 'input -> Task<'output>
    
type Command<'state, 'input, 'output> =
    Env -> 'state -> 'input -> Task<'output * 'state>
    

    