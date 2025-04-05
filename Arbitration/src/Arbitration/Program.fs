module Arbitration.Program

#nowarn "20"
open Arbitration.Application.CompositionRoot
open Arbitration.Application.ProjectConfig
open Arbitration.Jobs.SpreadJob
open Arbitration.Migrations
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Options

   
[<EntryPoint>]
let main args =
    task {
        let builder = WebApplication.CreateBuilder(args)
        configureServices builder
        let app = builder.Build()
        
        let connectionString = app.Services.GetRequiredService<IOptions<AppConfig>>().Value.Postgres.ConnectionString
        do! Migration.applyMigrations connectionString
        let env = configureApp app
        
        do startSpreadJob env
        do! app.RunAsync()
        return 0
    }
    |> Async.AwaitTask
    |> Async.RunSynchronously