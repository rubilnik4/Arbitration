module Arbitration.Composition.CompositionConfig

open Arbitration.Application.Configurations
open Arbitration.Application.Configurations.TelemetryConfigs
open Arbitration.Application.ProjectConfig
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection

[<Literal>]
let OTelSection = "OpenTelemetry"

let private getConfiguration (builder: WebApplicationBuilder) =
    ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional=true)
        .AddEnvironmentVariables()
        .Build()
        
let configureOptions (builder: WebApplicationBuilder) =
    let configuration = getConfiguration builder
    builder.Services        
        .AddOptions<AppConfig>()
        .Bind(configuration.GetSection("AppConfig"))      
        .ValidateDataAnnotations()
        .ValidateOnStart()
    |> ignore
    
    builder.Services   
        .AddOptions<OpenTelemetryConfig>()
        .Bind(configuration.GetSection(OTelSection))      
        .ValidateDataAnnotations()
        .ValidateOnStart()
    |> ignore  