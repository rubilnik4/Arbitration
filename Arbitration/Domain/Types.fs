module Arbitration.Domain.Types

open Arbitration.Domain.Models.Prices
open Arbitration.Domain.Models.Spreads

type MarketError =   
    | NotFound of string 
    | DatabaseError of message: string 
    | DatabaseNotFound of message: string
    | CacheError of message: string 
    | CacheNotFound of message: string
    | ApiError of provider: string * code: int * message: string 
    | ServiceUnavailable of service: string 
    | Unknown
    
type MarketResult<'a> = Result<'a, MarketError>

type PriceResult = MarketResult<Price>

type SpreadIdResult = MarketResult<SpreadId>

type SpreadResult = MarketResult<Spread>