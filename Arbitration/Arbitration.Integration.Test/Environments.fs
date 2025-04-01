module Arbitration.Integration.Test.Environments

open Arbitration.Application.Interfaces
open Arbitration.Infrastructure.MarketApi
open Arbitration.Infrastructure.MarketCache
open Arbitration.Infrastructure.MarketData
open Arbitration.Infrastructure.MarketRepository
open Arbitration.Integration.Test.Configurations
open Binance.Net.Clients
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Caching.Memory
open Moq
open Npgsql
open Testcontainers.PostgreSql

let getPostgresContainer() = 
    PostgreSqlBuilder()
        .WithImage("postgres:15")
        .WithDatabase("testDb")
        .WithUsername("testUser")
        .WithPassword("testPass")
        .Build()
                    
let getLogger() =
    let loggerMock = Mock<ILogger>()
    loggerMock.Object

let getInfra connectionString : Infra = {
    Logger = getLogger()
    Cache = new MemoryCache(MemoryCacheOptions())
    Config = getConfig connectionString
    Postgres = NpgsqlDataSource.Create connectionString  
    BinanceRestClient = new BinanceRestClient()
}

let createEnvironment connectionString = {
    Infra = getInfra connectionString
    Repository = postgresSpreadRepository
    Api = binanceMarketApi
    Data = marketData
    Cache = memoryMarketCache   
}