module Arbitration.Application.Dto.DtoModels

open System.ComponentModel.DataAnnotations
open Arbitration.Domain.Models.Assets

[<CLIMutable>]
type AssetSpreadRequest = {
    [<Required>]
    AssetA: AssetId
    
    [<Required>]
    AssetB: AssetId
}