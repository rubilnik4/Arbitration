module Arbitration.Application.Interfaces

open System
open System.Threading.Tasks
open Arbitration.Domain.Models
open Arbitration.Domain.Types
open Npgsql

type MarketData = {
    GetPrice: AssetId -> Task<PriceResult>
    GetLastPrice: AssetId -> Task<PriceResult>
}

type SpreadRepository = {
    SaveSpread: Spread -> Task<SpreadId>
    GetLastPrice: AssetId -> DateTimeOffset -> Task<Option<Price>>
}

type Cache ={
    Set: string -> decimal -> unit
}

type Command<'env, 'state, 'input, 'output> =
    'env -> 'state -> 'input -> Task<'output * 'state>
    