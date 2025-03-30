module Arbitration.Application.Queries.SpreadQuery

open System.Threading.Tasks
open Arbitration.Application.Interfaces
open Arbitration.Application.Queries.Queries
open Arbitration.Domain.Models
open Arbitration.Domain.Models.Assets
open Arbitration.Domain.Types

let spreadQuery : SpreadQuery =
    fun env input ->
        normalizeAsset input
        |> env.MarketData.GetLastSpread env 