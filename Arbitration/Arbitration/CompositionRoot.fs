module Arbitration.Application.CompositionRoot

open System
open Arbitration.Application.Configurations
open Arbitration.Application.Interfaces
open Arbitration.Controllers.PriceEndpoint
open Arbitration.Controllers.SpreadEndpoint
open Arbitration.Infrastructure.MarketCache
open Arbitration.Infrastructure.MarketData
open Arbitration.Infrastructure.MarketApi
open Arbitration.Infrastructure.MarketRepository
open Binance.Net.Interfaces.Clients
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Caching.Memory
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open Npgsql
open Oxpecker

let private configuration =
    ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional = false)
        .AddEnvironmentVariables()
        .Build()
   
let private createInfra (services: IServiceProvider) = 
    let config = services.GetRequiredService<AppConfig>()
    {
        Postgres = NpgsqlDataSource.Create config.Postgres.ConnectionString   
        BinanceRestClient = services.GetRequiredService<IBinanceRestClient>()
        Cache = services.GetRequiredService<IMemoryCache>()
        Logger = services.GetRequiredService<ILogger>()    
        Config = config
    }

let private createEnv (services: IServiceProvider) = {
    Infra = createInfra services
    Repository = postgresSpreadRepository
    Api = binanceMarketApi
    Data = marketData
    Cache = memoryMarketCache   
}  
    
let private getEndpoints env =
    List.Empty
    |> List.append (priceEndpoints env)
    |> List.append (spreadEndpoints env)
    
let private configureOptions (services: IServiceCollection) =
    services
        .AddOptions<AppConfig>()
        .Bind(configuration.GetSection("AppConfig"))      
        .ValidateDataAnnotations()
        .ValidateOnStart()
    |> ignore
    
let configureApp (appBuilder: IApplicationBuilder) =
    let env = createEnv(appBuilder.ApplicationServices)
    
    appBuilder
        .UseRouting()
        .UseOxpecker(getEndpoints env)
    |> ignore

let configureServices (services: IServiceCollection) =
    services
        .AddRouting()
        .AddOxpecker()
        .AddBinance()
        .AddMemoryCache()       
        .AddEndpointsApiExplorer()
        .Configure<AppConfig>(configuration.GetSection("AppConfig"))
    |> ignore
    configureOptions services