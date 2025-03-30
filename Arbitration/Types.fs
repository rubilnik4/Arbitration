module Arbitration.Domain.Types

open Arbitration.Domain.Models.Prices
open Arbitration.Domain.Models.Spreads

type ArbitrationResult<'a> = Result<'a, string>

type PriceResult = ArbitrationResult<Price>

type SpreadIdResult = ArbitrationResult<SpreadId>

type SpreadResult = ArbitrationResult<Spread>