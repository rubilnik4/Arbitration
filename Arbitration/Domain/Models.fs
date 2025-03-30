module Arbitration.Domain.Models

open System

type AssetId = string

type SpreadId = Guid

type Price = {
    Asset: AssetId
    Value: decimal
    Time: DateTime
}

type SpreadAsset = {
    AssetA: AssetId
    AssetB: AssetId
}

type Spread = {
    PriceA: Price
    PriceB: Price
    Value: decimal
    Time: DateTime
}

type SpreadState = {
    LastSpread: decimal option
    SpreadHistory: decimal list
    IsThresholdExceeded: bool
}
with
    static member Empty = {
        LastSpread = None
        SpreadHistory = []
        IsThresholdExceeded = false
    }