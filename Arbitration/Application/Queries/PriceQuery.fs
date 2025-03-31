module Arbitration.Application.Queries.PriceQuery

open Arbitration.Application.Environments.ApplicationTypes
open Arbitration.Application.Interfaces
open Arbitration.Domain.Models.Assets
open Arbitration.Domain.DomainTypes
open Microsoft.Extensions.Logging

let priceQuery : Query<AssetId, PriceResult> =
    fun env assetId ->
        env.Infra.Logger.LogInformation("Execute price query for asset: {assetId}", assetId)
        env.Data.GetLastPrice env assetId