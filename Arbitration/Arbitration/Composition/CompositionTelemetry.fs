module Arbitration.Composition.CompositionTelemetry

open System
open Arbitration.Application.Configurations.TelemetryConfigs
open Arbitration.Composition.CompositionConfig
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open OpenTelemetry.Trace
open OpenTelemetry.Metrics
open OpenTelemetry.Logs

let configureTelemetry (builder: WebApplicationBuilder) =
    let oTelConfig = builder.Configuration.GetSection(OTelSection).Get<OpenTelemetryConfig>()
    
    builder.Services
        .AddLogging(fun logging ->
            logging.AddOpenTelemetry(fun opt ->
                opt.AddOtlpExporter(fun exporterOpt ->
                    exporterOpt.Endpoint <- oTelConfig.Loki.LokiEndpoint |> Uri)
                |> ignore)
            |> ignore)
    |> ignore
    
    builder.Services
        .AddOpenTelemetry()
        .WithTracing(fun builder ->          
            builder
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddOtlpExporter(fun opt ->
                    opt.Endpoint <- oTelConfig.Tracing.OtlpEndpoint |> Uri)
            |> ignore)
        .WithMetrics(fun builder ->
            builder
                .AddRuntimeInstrumentation() 
                .AddPrometheusExporter(fun opt ->
                    opt.ScrapeEndpointPath <- oTelConfig.Metrics.PrometheusEndpoint)
            |> ignore)
        |> ignore