module Arbitration.Domain.Models

open System

type Asset = {
    Asset: string
    Value: decimal
}

type Spread = {
    AssetA: string
    AssetB: string
    Value: decimal
    Time: DateTimeOffset
}

type SpreadState = {
    LastSpread: decimal option
    SpreadHistory: decimal list
    IsThresholdExceeded: bool
}
