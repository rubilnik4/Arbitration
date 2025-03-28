module Arbitration.Domain.Types

open Arbitration.Domain.Models

type SpreadQuery =
    | GetLastSpread
    | GetHistory

type SpreadInput = {
    AssetA: AssetId
    AssetB: AssetId
}

type ArbitrationResult<'a> = Result<'a, string>

type PriceResult = ArbitrationResult<Price>

type SpreadIdResult = ArbitrationResult<SpreadId>

type SpreadResult = ArbitrationResult<Spread>