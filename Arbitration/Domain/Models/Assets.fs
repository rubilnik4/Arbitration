module Arbitration.Domain.Models.Assets

type AssetId = string

type AssetSpread = {
    AssetA: AssetId
    AssetB: AssetId
}

let normalizeAsset assetSpread =
    let sorted = [assetSpread.AssetA; assetSpread.AssetB] |> List.sort
    { AssetA = sorted[0]; AssetB = sorted[1] }