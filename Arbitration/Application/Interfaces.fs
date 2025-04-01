module Arbitration.Application.Interfaces

open Arbitration.Domain.Models.Assets
open Arbitration.Domain.Models.Prices
open Arbitration.Domain.Models.Spreads
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Caching.Memory
open Binance.Net.Interfaces.Clients
open Npgsql
open Arbitration.Application.Configurations
open System.Threading.Tasks
open Arbitration.Domain.DomainTypes

type Infra = {
    Logger: ILogger
    Cache: IMemoryCache
    Config: Config
    Postgres: NpgsqlDataSource
    BinanceRestClient: IBinanceRestClient
}

type MarketApi = {
    GetPrice: Infra -> AssetId -> Task<PriceResult>
}

type MarketRepository = {
    SaveSpread: Infra -> Spread -> Task<SpreadIdResult>
    GetLastPrice: Infra -> AssetId -> Task<PriceResult>
    GetLastSpread: Infra -> AssetSpreadId -> Task<SpreadResult>
}

type Cache<'TKey, 'T> = {
    TryGet: Infra -> 'TKey -> MarketResult<'T>
    Set: Infra -> 'T -> MarketResult<'T>
    Remove: Infra -> 'TKey -> MarketResult<unit>
}

type MarketCache = {
    LastPrice : Cache<AssetId, Price>
    LastSpread : Cache<AssetSpreadId, Spread>
}

type Env = {
    Infra: Infra
    Repository: MarketRepository
    Api: MarketApi
    Cache: MarketCache
    Data: MarketData
}
and MarketData = {
    GetPrice: Env -> AssetId -> Task<PriceResult>
    GetLastPrice: Env -> AssetId -> Task<PriceResult>
    GetLastSpread: Env -> AssetSpreadId -> Task<SpreadResult>
}



