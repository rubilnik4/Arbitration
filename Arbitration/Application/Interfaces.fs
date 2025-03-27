module Arbitration.Application.Interfaces

open System.Threading.Tasks
open Arbitration.Domain.Models
open FSharpPlus.Data

type SpreadResult<'a> = Result<'a, string>

type MarketData = {
    GetPrice: string -> Async<SpreadResult<Asset>>
    GetLastPrice: string -> Async<SpreadResult<Asset>>
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

type SpreadResultT<'a> = ResultT<Async<'a>>

type SpreadStateT<'a> = StateT<SpreadState, SpreadResultT<'a>>

type SpreadAppT<'a> = ReaderT<Env, SpreadStateT<'a>>
