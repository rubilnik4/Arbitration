module Arbitration.Application.Queries.SpreadQuery

open Arbitration.Application.Environments.ApplicationTypes
open Arbitration.Application.Interfaces
open Arbitration.Domain.Models.Assets
open Arbitration.Domain.DomainTypes
open Microsoft.Extensions.Logging

let spreadQuery : Query<AssetSpreadId, SpreadResult> =
    fun env input ->
        let assetSpreadId = normalizeSpreadAsset input
        env.Infra.Logger.LogInformation("Execute spread query for assets: {assetSpreadId}", assetSpreadId)
        env.Data.GetLastSpread env assetSpreadId