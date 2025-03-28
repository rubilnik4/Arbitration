module Arbitration.Domain.Models

open System

type AssetId = string

type SpreadId = string

type Price = {
    Asset: AssetId
    Value: decimal
}

type Spread = {
    AssetA: AssetId
    AssetB: AssetId
    Value: decimal
    Time: DateTimeOffset
}

type SpreadState = {
    LastSpread: decimal option
    SpreadHistory: decimal list
    IsThresholdExceeded: bool
}
