module Arbitration.Controllers.PriceEndpoint

open Arbitration.Application.Interfaces
open Arbitration.Application.Queries.PriceQuery
open Arbitration.Controllers.Routes
open Arbitration.Domain.DomainTypes
open Microsoft.Extensions.Logging
open Oxpecker
open type Microsoft.AspNetCore.Http.TypedResults

let private pricePath =
    getRoute PriceRoute

let private getLastPrice env : EndpointHandler = 
    fun ctx -> task {
        match ctx.TryGetQueryValue "asset" with
        | Some asset ->               
            env.Infra.Logger.LogInformation("Received request for asset price: {Asset}", asset)

            let! result = priceQuery env asset
            
            return!
                match result with
                | Ok price ->
                    env.Infra.Logger.LogInformation("Price found for asset {Asset}: {Price}", asset, price)
                    ctx.Write <| Ok price
                | Error (NotFound message) ->
                    env.Infra.Logger.LogWarning("Price not found for asset {Asset}: {Message}", asset, message)
                    ctx.Write <| NotFound {| Message = message |}
                | Error error ->
                    env.Infra.Logger.LogError("Error retrieving price for asset {Asset}: {Error}", asset, error)
                    ctx.Write <| InternalServerError {| Error = error |}
        | None ->
            env.Infra.Logger.LogWarning("AssetId query parameter is missing")
            return! ctx.Write <| BadRequest "AssetId not found"        
    }
    
let priceEndpoints env = [
    subRoute pricePath [
        GET [ route "/" <| getLastPrice env ]
    ]
]