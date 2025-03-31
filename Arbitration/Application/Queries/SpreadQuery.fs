module Arbitration.Application.Queries.SpreadQuery

open Arbitration.Application.Interfaces
open Arbitration.Domain.Models.Assets
open Arbitration.Domain.Types
open Microsoft.Extensions.Logging

let spreadQuery : Query<AssetSpreadId, SpreadResult> =
    fun env input ->
        let assetSpreadId = normalizeSpreadAsset input
        env.Logger.LogInformation("Execute spread query for assets: {assetSpreadId}", assetSpreadId)
        env.MarketData.GetLastSpread env assetSpreadId