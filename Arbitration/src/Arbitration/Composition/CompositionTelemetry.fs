module Arbitration.Composition.CompositionTelemetry

open System
open System.Collections.Generic
open Arbitration.Application.Configurations.TelemetryConfigs
open Arbitration.Composition.CompositionConfig
open Arbitration.Infrastructure.Activities
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open OpenTelemetry.Exporter
open OpenTelemetry.Resources
open OpenTelemetry.Trace
open OpenTelemetry.Metrics
open OpenTelemetry.Logs

let configureTelemetry (builder: WebApplicationBuilder) =
    let oTelConfig = builder.Configuration.GetSection(OTelSection).Get<OpenTelemetryConfig>()
    
    builder.Services
        .AddLogging(fun logging ->
            logging
                .AddOpenTelemetry(fun opt ->
                    opt.IncludeScopes <- true;
                    opt.ParseStateValues <- true;
                    opt
                        .SetResourceBuilder(
                            ResourceBuilder.CreateDefault().AddService(ActivityName))
                        .AddOtlpExporter(fun exporterOpt ->
                            exporterOpt.Endpoint <- oTelConfig.Logging.LokiEndpoint |> Uri)
                    |> ignore)
            |> ignore)
    |> ignore
    
    builder.Services
        .AddOpenTelemetry()
        .WithTracing(fun builder ->          
            builder              
                .AddSource(ActivityName)                
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