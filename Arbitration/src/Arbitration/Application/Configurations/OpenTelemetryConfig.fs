module Arbitration.Application.Configurations.TelemetryConfigs

open System.ComponentModel.DataAnnotations

[<CLIMutable>]
type LokiConfig = {
    [<Required>]
    LokiEndpoint: string
}

[<CLIMutable>]
type TracingConfig = {
    [<Required>]
    OtlpEndpoint: string
}

[<CLIMutable>]
type MetricsConfig = {
    [<Required>]
    PrometheusEndpoint: string
}

[<CLIMutable>]
type OpenTelemetryConfig = {
    [<Required>]
    Loki: LokiConfig
    
    [<Required>]
    Tracing: TracingConfig
    
    [<Required>]
    Metrics: MetricsConfig
}