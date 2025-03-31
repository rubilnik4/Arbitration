module Arbitration.Domain.Types

open Arbitration.Domain.Models.Assets
open Arbitration.Domain.Models.Prices
open Arbitration.Domain.Models.Spreads
    
type MarketResult<'a> = Result<'a, string>

type PriceResult = MarketResult<Price>

type SpreadIdResult = MarketResult<SpreadId>

type SpreadResult = MarketResult<Spread>