module Arbitration.Application.Dto.DtoModels

open System.ComponentModel.DataAnnotations
open Arbitration.Domain.Models

[<CLIMutable>]
type SpreadAssetRequest = {
    [<Required>]
    AssetA: AssetId
    
    [<Required>]
    AssetB: AssetId
}