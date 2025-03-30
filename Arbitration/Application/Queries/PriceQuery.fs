module Arbitration.Application.Queries.PriceQuery

open Arbitration.Application.Interfaces
open Arbitration.Application.Queries.Queries

let priceQuery : PriceQuery =
    fun env input ->
        env.MarketData.GetLastPrice env input