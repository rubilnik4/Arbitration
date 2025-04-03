module Arbitration.Infrastructure.Activities

open System
open System.Diagnostics
open System.Diagnostics.Metrics
open Arbitration.Domain.DomainTypes
open Microsoft.Extensions.Logging

[<Literal>]
let private ServiceName = "ArbitrationService"

let private activitySource = new ActivitySource("Arbitration")

let private meter = new Meter("Arbitration")

let private spreadDuration = meter.CreateHistogram<float>("spread.duration", "ms")

let startActivity (name: string) =
    let parentContext = 
        match Activity.Current with
        | null -> ActivityContext()
        | current -> current.Context

    let activity = activitySource.StartActivity(name, ActivityKind.Internal, parentContext)
    activity

let configureActivity (tags: (string * obj) list) (activity: Activity) =   
    activity.SetTag("service.name", ServiceName) |> ignore
    tags |> List.iter (fun (k, v) -> activity.SetTag(k, v) |> ignore)
    activity

let recordSuccess (logger: ILogger) (activity: Activity) =    
    activity.SetTag("result.status", "success") |> ignore
    logger.LogInformation("Activity {ActivityName} completed", activity.OperationName)
    activity

let recordError (logger: ILogger) (error: MarketError) (activity: Activity) =    
    activity.SetTag("result.status", "error") |> ignore
    activity.SetTag("error.message", error) |> ignore
    logger.LogError("Activity {ActivityName} error: {Error}", activity.OperationName, error)
    activity
    
let recordException (logger: ILogger) (ex: exn) (activity: Activity) =    
    let tags = ActivityTagsCollection()
    tags.Add("exception.type", ex.GetType().Name)
    tags.Add("exception.message", ex.Message)
    activity.AddEvent(ActivityEvent("exception", tags = tags)) |> ignore
    logger.LogError(ex, "Activity {ActivityName} failed", activity.OperationName)
    activity

let recordDuration (activity: Activity) =    
    spreadDuration.Record(activity.Duration.TotalMilliseconds)
    activity    

let disposeActivity (activity: Activity) =
    activity.Dispose()    