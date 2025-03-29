module Arbitration.Controllers.SpreadController

open Arbitration.Application.Commands.ComputeSpread
open Arbitration.Application.Interfaces
open Arbitration.Controllers.Controller
open Arbitration.Domain.Models
open Arbitration.Domain.Types
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Http.HttpResults
open Oxpecker
open type Microsoft.AspNetCore.Http.TypedResults

let private spreadPath =
    getRoute spreadRoute

let getLastSpreadController env : EndpointHandler = 
    fun ctx -> task {
        let marketData = env.MarketData
        let result = marketData.GetLastPrice
        return! ctx.Response.WriteAsync("Last spread details")
    }
        
let computeSpreadController env : EndpointHandler  =
    fun ctx -> task {
        let assetsConfig = env.Config.Project.Assets
        let input = { AssetA = assetsConfig.AssetA; AssetB = assetsConfig.AssetB  }
        let state = SpreadState.Empty
        let! result, _ = computeSpreadCommand env state input       
        
        return!
            match result with
            | Ok spread -> ctx.Write <| Ok spread
            | Error error -> ctx.Write <| BadRequest {| Error = error |}
    }

let webApp env = [
    subRoute spreadPath [
        GET [ route "/" <| getLastSpreadController env ]
        POST [ route "/" <| computeSpreadController env ]
    ]
]