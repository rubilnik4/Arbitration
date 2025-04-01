module Arbitration.Composition.OptionsConfig

open Arbitration.Application.Configurations
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection

let private configuration =
    ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional = false)
        .AddEnvironmentVariables()
        .Build()
        
let configureOptions (services: IServiceCollection) =
    services
        .Configure<AppConfig>(configuration.GetSection("AppConfig"))
        .AddOptions<AppConfig>()
        .Bind(configuration.GetSection("AppConfig"))      
        .ValidateDataAnnotations()
        .ValidateOnStart()
    |> ignore