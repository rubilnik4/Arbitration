module Arbitration.Domain.Models.Spreads

open System
open Arbitration.Domain.Models.Assets
open Arbitration.Domain.Models.Prices

type SpreadId = Guid

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

let getAssetSpreadId spread =
    AssetSpreadId(spread.PriceA.Asset, spread.PriceB.Asset)
    
let getSpreadKey spread =
    let assetSpreadId = getAssetSpreadId spread
    getAssetSpreadKey assetSpreadId 