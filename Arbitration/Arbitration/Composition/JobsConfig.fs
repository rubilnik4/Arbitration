module Arbitration.Composition.JobsConfig

open Arbitration.Application.Interfaces
open Arbitration.Domain.Models.Spreads
open Arbitration.Jobs
open Arbitration.Jobs.SpreadJob
open Microsoft.Extensions.DependencyInjection
open Quartz
open Quartz.Simpl

// let configureJobs (services: IServiceCollection) =
//     services
//         .AddQuartz(fun options ->
//             let jobKey = JobKey SpreadJobName
//             options
//                 .AddJob<SpreadJob>(fun job -> job.WithIdentity(jobKey) |> ignore)
//                 // .AddJob<SpreadJob>(fun job ->
//                 //     job.WithIdentity(jobKey) |> ignore
//                 //     job.UsingJobData("SpreadState", SpreadState.Empty) |> ignore 
//                 // )
//                 // .AddTrigger(fun trigger ->
//                 //     trigger
//                 //         .ForJob(jobKey)
//                 //         .WithIdentity("ComputeSpreadTrigger")
//                 //         .WithSimpleSchedule(fun schedule ->
//                 //             schedule
//                 //                 .WithIntervalInSeconds(10) // Каждые 10 сек
//                 //                 .RepeatForever()
//                 //             |> ignore)
//                 //     |> ignore)
//         )
//         .AddQuartzHostedService(fun options -> options.WaitForJobsToComplete <- true)
//     |> ignore

let configureQuartz (services: IServiceCollection, env: Env) =
    services
        .AddQuartz(fun options ->
            options
                .AddJob<SpreadJob>(fun job ->
                    job.WithIdentity(SpreadJobName) |> ignore
                    //job.UsingJobData(SpreadStateName, SpreadState.Empty) |> ignore 
                )
                .AddTrigger(fun trigger ->
                    trigger
                        .ForJob(SpreadJobName)
                        .WithIdentity("SpreadTrigger")
                        .WithSimpleSchedule(fun schedule ->
                            schedule
                                .WithIntervalInSeconds(10)
                                .RepeatForever()
                            |> ignore)
                    |> ignore)
            |> ignore    
        )
        .AddQuartzHostedService(fun options -> options.WaitForJobsToComplete <- true)
    |> ignore