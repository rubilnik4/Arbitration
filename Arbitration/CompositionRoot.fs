module Arbitration.Application.CompositionRoot

open System
open Arbitration.Application.Configurations
open Arbitration.Application.Interfaces
open Arbitration.Controllers.PriceEndpoint
open Arbitration.Controllers.SpreadEndpoint
open Arbitration.Infrastructure.MarketData
open Arbitration.Infrastructure.SpreadApi
open Arbitration.Infrastructure.SpreadRepository
open Binance.Net.Interfaces.Clients
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open Microsoft.OpenApi.Models
open Npgsql
open Oxpecker

let private config =
    ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .AddEnvironmentVariables()
        .Build()
        .Get<Config>()
        
let postgresEnv = {
    Source = NpgsqlDataSource.Create config.Postgres.ConnectionString
}          
        
let private createEnv (services: IServiceProvider) = {
    Postgres = postgresEnv
    SpreadRepository = postgresSpreadRepository
    SpreadApi = binanceSpreadApi
    MarketData = marketData
    BinanceRestClient = services.GetRequiredService<IBinanceRestClient>()
    Logger = services.GetRequiredService<ILogger>()
    Cache = Unchecked.defaultof<Cache>
    Config = config
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