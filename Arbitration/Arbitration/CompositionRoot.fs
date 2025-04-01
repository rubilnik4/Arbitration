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
open Microsoft.OpenApi.Models
open Npgsql
open Oxpecker

let private getConfig() =
    ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .AddEnvironmentVariables()
        .Build()
        .Get<Config>()
   
let private createInfra (services: IServiceProvider) = 
    let config = services.GetRequiredService<Config>()
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
    
let getEndpoints env =
    List.Empty
    |> List.append (priceEndpoints env)
    |> List.append (spreadEndpoints env)
    
let configureApp (appBuilder: IApplicationBuilder) =
    let env = createEnv(appBuilder.ApplicationServices)
    
    appBuilder
        .UseRouting()
        .UseOxpecker(getEndpoints env)
        .UseSwagger()
        .UseSwaggerUI(fun c ->
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Spread API V1")
            //c.RoutePrefix <- ""
        )
    |> ignore

let configureServices (services: IServiceCollection) =
    services
        .AddRouting()
        .AddOxpecker()
        .AddBinance()
        .AddMemoryCache()
        .AddSingleton(getConfig)
        .AddEndpointsApiExplorer()
        .AddSwaggerGen(fun c ->
            c.SwaggerDoc("v1", OpenApiInfo(
                Title = "Spread API",
                Version = "v1",
                Description = "Сервис для вычисления спреда между двумя активами"
            ))            
            //c.CustomSchemaIds _.Name
    )
    |> ignore