module Arbitration.Controllers.PriceEndpoint

open Arbitration.Application.Queries.PriceQuery
open Arbitration.Controllers.Routes
open Arbitration.Domain.Models.Assets
open Arbitration.Domain.DomainTypes
open Oxpecker
open type Microsoft.AspNetCore.Http.TypedResults

let private pricePath =
    getRoute PriceRoute

let private getLastPrice env : EndpointHandler = 
    fun ctx -> task {
        match ctx.TryGetQueryValue "asset" with
        | Some asset ->               
            let! result = priceQuery env (AssetId asset)
            return!
                match result with
                | Ok price ->
                    ctx.Write <| Ok price
                | Error (NotFound message) ->
                    ctx.Write <| NotFound {| Message = message |}
                | Error error ->
                    ctx.Write <| InternalServerError {| Error = error |}
        | None ->
            return! ctx.Write <| BadRequest "AssetId not found"        
    }
    
let priceEndpoints env = [
    subRoute pricePath [
        GET [ route "/" <| getLastPrice env ]
    ]
]