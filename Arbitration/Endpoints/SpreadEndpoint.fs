module Arbitration.Controllers.SpreadEndpoint

open Arbitration.Application.Commands.SpreadCommand
open Arbitration.Application.Dto.DtoModels
open Arbitration.Application.Queries.SpreadQuery
open Arbitration.Controllers.Routes
open Arbitration.Domain.Models.Assets
open Arbitration.Domain.Models.Spreads
open Arbitration.Domain.DomainTypes
open Oxpecker
open type Microsoft.AspNetCore.Http.TypedResults

let private spreadPath =
    getRoute SpreadRoute
    
let private getLastSpread env : EndpointHandler = 
    fun ctx -> task {
        match ctx.TryGetQueryValue "assetA", ctx.TryGetQueryValue "assetB"  with
        | Some assetA, Some assetB ->            
            let! result = spreadQuery env (assetA, assetB)
            return!
                match result with
                | Ok spread ->
                    ctx.Write <| Ok spread
                | Error (NotFound message) ->
                    ctx.Write <| NotFound {| Message = message |}
                | Error error ->
                    ctx.Write <| InternalServerError {| Error = error |}
        | _, _ ->
            return! ctx.Write <| BadRequest "AssetId not found"        
    }
        
let private computeSpread env : EndpointHandler  =
    fun ctx -> task {
        match! ctx.BindAndValidateJson<AssetSpreadRequest>() with
            | ModelValidationResult.Valid request ->
                let spreadAsset = AssetSpreadId (request.AssetA, request.AssetB )
                let state = SpreadState.Empty
                let! result, _ = spreadCommand env state spreadAsset       
                
                return!
                    match result with
                    | Ok spread ->
                        ctx.Write <| Ok spread
                    | Error error ->
                        ctx.Write <| InternalServerError {| Error = error |}
            | ModelValidationResult.Invalid (_, errors) ->
                return! ctx.Write <| BadRequest errors.All
    }

let spreadEndpoints env = [
    subRoute spreadPath [
        GET [ route "/" <| getLastSpread env ]
        POST [ route "/" <| computeSpread env ]
    ]
]