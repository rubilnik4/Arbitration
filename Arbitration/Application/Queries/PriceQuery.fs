module Arbitration.Application.Queries.PriceQuery

open Arbitration.Application.Interfaces
open Arbitration.Domain.Models.Assets
open Arbitration.Domain.Types
open Microsoft.Extensions.Logging

let priceQuery : Query<AssetId, PriceResult> =
    fun env assetId ->
        env.Logger.LogInformation("Execute price query for asset: {assetId}", assetId)
        env.MarketData.GetLastPrice env assetId