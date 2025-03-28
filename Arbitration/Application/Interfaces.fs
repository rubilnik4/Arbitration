module Arbitration.Application.Interfaces

open System.Threading.Tasks
open Arbitration.Domain.Models
open Arbitration.Domain.Types

type MarketData = {
    GetPrice: string -> Task<PriceResult>
    GetLastPrice: string -> Task<PriceResult>
}

type Storage = {
    SaveSpread: Spread -> unit
}

type Cache ={
    Set: string -> decimal -> unit
}

type Config = {
    SpreadThreshold: decimal
    MaxHistorySize: int
}

type Env = {
    Logger: string -> unit
    MarketData: MarketData
    Storage: Storage
    Cache: Cache
    Config: Config
}

type Command<'env, 'state, 'input, 'output> =
    'env -> 'state -> 'input -> Task<'output * 'state>
    