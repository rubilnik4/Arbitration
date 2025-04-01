module Arbitration.Jobs.SpreadJob

open Arbitration.Application.Commands.SpreadCommand
open Arbitration.Application.Interfaces
open Arbitration.Domain.Models.Assets
open Arbitration.Domain.Models.Spreads
open Quartz
open Microsoft.Extensions.Logging

[<Literal>]
let SpreadJobName = "SpreadJob"

[<Literal>]
let SpreadStateName = "SpreadState"

type SpreadJob =
    let getLastState (context: IJobExecutionContext) =
        match context.JobDetail.JobDataMap.ContainsKey(SpreadStateName) with
        | true  -> context.JobDetail.JobDataMap[SpreadStateName] :?> SpreadState
        | false -> SpreadState.Empty
        
    interface IJob with
        member _.Execute(context: IJobExecutionContext) = task {           
            let assets = env.Infra.Config.Project.Assets
            let spreadAssetId = AssetSpreadId(assets.AssetA, assets.AssetB)
            env.Infra.Logger.LogInformation("Execute spread job for assets: {Assets}", spreadAssetId)            
            
            let lastState = getLastState context 
            let! result, newState = spreadCommand env lastState spreadAssetId 
            
            match result with
            | Ok spread ->
                env.Infra.Logger.LogInformation("Successfully completed job for {Assets} spread {Spread}",
                                                spreadAssetId, spread)
                context.JobDetail.JobDataMap[SpreadStateName] <- newState
            | Error error ->
                env.Infra.Logger.LogError("Failed to compute spread: {Error}", error)
        }