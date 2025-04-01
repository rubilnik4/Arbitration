module Arbitration.Domain.Models.Prices

open System
open Arbitration.Domain.Models.Assets

type Price = {
    Asset: AssetId
    Value: decimal
    Time: DateTime
}