module Arbitration.Jobs.SpreadJob

open System.Diagnostics
open Arbitration.Application.Commands.SpreadCommand
open Arbitration.Application.Interfaces
open Arbitration.Domain.Models.Assets
open Arbitration.Domain.Models.Spreads
open Arbitration.Infrastructure.Activities
open Hopac
open Hopac.Infixes
open Microsoft.Extensions.Logging 

let private computeSpread env state = task {
    use activity = startActivity "Arbitration.ComputeSpread" |> configureActivity ["operation", "ComputeSpread"]

    try
        let spreadAssetId = AssetSpreadId(env.Infra.Config.Project.Assets.AssetA, env.Infra.Config.Project.Assets.AssetB)
        
        let! result, newState = spreadCommand env state spreadAssetId 
        
        match result with
        | Ok spread ->
            activity |> recordSuccess |> recordDuration |> configureActivity ["spread.value", spread] |> ignore
            
            return newState
        | Error error ->
            recordError activity error |> recordDuration |> ignore
            return state
    with ex ->
        recordException activity ex |> recordDuration |> ignore
        return state    
}    

let private spreadJob env = job {
    let stopCh = Ch<unit>() 
    let timerCh = Ch<unit>() 
   
    let rec timerLoop() = job {           
        do! timeOut env.Infra.Config.Project.AssetLoadingDelay
        do! Ch.send timerCh ()
        return! timerLoop()
    }
    
    let handleError (e: exn) state = job {
        env.Infra.Logger.LogError("Failed to execute job spread: {Error}", e.Message)
        return state
    }
 
    let rec processingLoop state = 
        Alt.choose [
            Ch.take timerCh ^=> fun _ ->
                Job.tryInDelay 
                    (fun () -> computeSpread env state |> Job.awaitTask)
                    processingLoop
                    (fun e -> handleError e state >>= processingLoop)
            
            Ch.take stopCh ^=> fun _ -> job { () }
        ]
       
    do! Job.start (timerLoop())
    do! processingLoop(SpreadState.Init)
    
    return stopCh
}
    
let startSpreadJob env = 
    spreadJob env |> Job.startIgnore |> run