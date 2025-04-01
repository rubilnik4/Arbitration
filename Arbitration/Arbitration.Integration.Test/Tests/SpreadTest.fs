module Arbitration.Integration.Test.SpreadTest

open Arbitration.Application.Commands.SpreadCommand
open Arbitration.Application.Interfaces
open Arbitration.Domain.Models.Assets
open Arbitration.Domain.Models.Spreads
open Arbitration.Infrastructure.MarketRepository
open Arbitration.Integration.Test.Environments
open Arbitration.Migrations
open FsUnit
open NUnit.Framework
open Testcontainers.PostgreSql

type TestEnvironment() =   
    let mutable postgresContainer = Unchecked.defaultof<PostgreSqlContainer>
    let mutable Env = Unchecked.defaultof<Env>

    [<OneTimeSetUp>]
    member _.SetUp() =
        task {
            printfn "Setting up test environment..."
           
            let container = getPostgresContainer()            
            do! container.StartAsync()          
            postgresContainer <- container
            let connectionString = container.GetConnectionString()
            do! Migration.applyMigrations connectionString

            let env = createEnvironment connectionString
            Env <- env
        }
        |> Async.AwaitTask
        |> Async.RunSynchronously
        
    [<Test; Order(1)>]
    member _.``Should save spread successfully``() = task {
        let project = Env.Infra.Config.Project
        let spreadAssetId = AssetSpreadId (project.Assets.AssetA, project.Assets.AssetB)
        
        let! result, _ = spreadCommand Env SpreadState.Empty spreadAssetId
        
        match result with
        | Ok spread ->
            spread.Value |> should be (greaterThan 0m)
            spread.Value |> should equal ((spread.PriceA.Value - spread.PriceB.Value) |> abs)
            getAssetSpreadId spread |> should equal (spreadAssetId |> normalizeSpreadAsset)
        | Error e ->
            failwith $"Failed compute spread: {e}"
    }

    [<Test; Order(2)>]
    member _.``Should retrieve last price for asset``() = task {
        let project = Env.Infra.Config.Project
        
        let! result = getLastPrice Env.Infra project.Assets.AssetA
        
        match result with
        | Ok price ->
            price.Asset |> should equal project.Assets.AssetA
            price.Value |> should be (greaterThan 0m)
        | Error e ->
            failwith $"Failed get last price: {e}"
    }
    
    [<Test; Order(2)>]
    member _.``Should retrieve last spread``() = task {
        let project = Env.Infra.Config.Project
        let spreadAssetId = AssetSpreadId (project.Assets.AssetA, project.Assets.AssetB)
        
        let! result = getLastSpread Env.Infra spreadAssetId
        
        match result with
        | Ok spread ->            
            spread.Value |> should be (greaterThan 0m)
        | Error e ->
            failwith $"Failed get last spread: {e}"
    }
    
    [<OneTimeTearDown>]
    member _.TearDown() =
        task {
            printfn "Cleaning up test environment..."
            
            do! postgresContainer.StopAsync()
        }
        |> Async.AwaitTask
        |> Async.RunSynchronously