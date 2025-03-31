module Arbitration.Domain.Models.Assets

type AssetId = string

type AssetSpreadId = AssetId * AssetId

let normalizeSpreadAsset (a, b) =
    if a < b then AssetSpreadId(a, b) else AssetSpreadId(b, a)
    
let getAssetSpreadKey assetSpread =
    let a, b = normalizeSpreadAsset assetSpread
    $"{a}|{b}"