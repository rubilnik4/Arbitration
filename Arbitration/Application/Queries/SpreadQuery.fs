module Arbitration.Application.Queries.SpreadQuery

open System.Threading.Tasks
open Arbitration.Application.Interfaces
open Arbitration.Application.Queries.Queries
open Arbitration.Domain.Models
open Arbitration.Domain.Types

let spreadQuery : SpreadQuery =
    fun env input ->
        env.MarketData.GetLastSpread env input