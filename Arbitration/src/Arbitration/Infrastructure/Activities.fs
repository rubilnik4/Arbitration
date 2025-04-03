module Arbitration.Infrastructure.Activities

open System.Diagnostics
open Arbitration.Domain.DomainTypes

[<Literal>]
let private ServiceName = "ArbitrationService"

let private activitySource = new ActivitySource("Arbitration")

let startActivity (name: string) =
    let parentContext = 
        match Activity.Current with
        | null -> ActivityContext()
        | current -> current.Context

    activitySource.StartActivity(name, ActivityKind.Internal, parentContext)

let configureActivity (tags: (string * obj) list) (activity: Activity) =   
    activity.SetTag("service.name", ServiceName) |> ignore
    tags |> List.iter (fun (k, v) -> activity.SetTag(k, v) |> ignore)
    activity

let recordSuccess (activity: Activity) =    
    activity.SetTag("result.status", "success") |> ignore
    activity

let recordError (activity: Activity) (errorMsg: MarketError) =    
    activity.SetTag("result.status", "error") |> ignore
    activity.SetTag("error.message", errorMsg) |> ignore
    activity
    
let recordException (activity: Activity) (ex: exn) =
    if activity <> null then
        let tags = ActivityTagsCollection()
        tags.Add("exception.type", ex.GetType().Name)
        tags.Add("exception.message", ex.Message)
        activity.AddEvent(ActivityEvent("exception", tags = tags)) |> ignore
    activity

let disposeActivity (activity: Activity) =
    activity.Dispose()    