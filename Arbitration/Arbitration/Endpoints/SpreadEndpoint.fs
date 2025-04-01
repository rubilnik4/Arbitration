module Arbitration.Controllers.SpreadEndpoint

open Arbitration.Application.Commands.SpreadCommand
open Arbitration.Application.Dto.DtoModels
open Arbitration.Application.Interfaces
open Arbitration.Application.Queries.SpreadQuery
open Arbitration.Controllers.Routes
open Arbitration.Domain.Models.Assets
open Arbitration.Domain.Models.Spreads
open Arbitration.Domain.DomainTypes
open Microsoft.Extensions.Logging
open Oxpecker
open type Microsoft.AspNetCore.Http.TypedResults

let private spreadPath =
    getRoute SpreadRoute
    
let private getLastSpread env : EndpointHandler = 
    fun ctx -> task {
        match ctx.TryGetQueryValue "assetA", ctx.TryGetQueryValue "assetB" with
        | Some assetA, Some assetB ->
            let spreadAssetId = AssetSpreadId(assetA, assetB)
            env.Infra.Logger.LogInformation("Fetching last spread for assets: {Assets}", spreadAssetId)
            
            let! result = spreadQuery env spreadAssetId
            
            return!
                match result with
                | Ok spread ->
                    env.Infra.Logger.LogInformation("Successfully fetched spread for {Assets}: {Spread}", spreadAssetId, spread)
                    ctx.Write <| Ok spread
                | Error (NotFound message) ->
                    env.Infra.Logger.LogWarning("Spread not found for {Assets}: {Message}", spreadAssetId, message)
                    ctx.Write <| NotFound {| Message = message |}
                | Error error ->
                    env.Infra.Logger.LogError("Failed to fetch spread for {Assets}: {Error}", spreadAssetId, error)
                    ctx.Write <| InternalServerError {| Error = error |}
        | _, _ ->
            env.Infra.Logger.LogWarning("Bad request: Missing assets")
            return! ctx.Write <| BadRequest "AssetId not found"        
    }

let private computeSpread env : EndpointHandler  =
    fun ctx -> task {       
        match! ctx.BindAndValidateJson<AssetSpreadRequest>() with
        | ModelValidationResult.Valid request ->
            let spreadAssetId = AssetSpreadId(request.AssetA, request.AssetB)
            env.Infra.Logger.LogInformation("Computing spread for assets: {Assets}", spreadAssetId)            
            
            let! result, _ = spreadCommand env SpreadState.Empty spreadAssetId       

            return!
                match result with
                | Ok spread ->
                    env.Infra.Logger.LogInformation("Successfully computed spread for {Assets}", spreadAssetId)
                    ctx.Write <| Ok spread
                | Error error ->
                    env.Infra.Logger.LogError("Failed to compute spread: {Error}", error)
                    ctx.Write <| InternalServerError {| Error = error |}
        | ModelValidationResult.Invalid (_, errors) ->
            env.Infra.Logger.LogWarning("Invalid request: {Errors}", errors.All)
            return! ctx.Write <| BadRequest errors.All
    }

let spreadEndpoints env = [
    subRoute spreadPath [
        GET [ route "/" <| getLastSpread env ]
        POST [ route "/" <| computeSpread env ]
    ]
]