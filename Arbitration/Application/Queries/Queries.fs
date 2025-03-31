module Arbitration.Application.Queries.Queries

open Arbitration.Application.Interfaces
open Arbitration.Domain.Models.Assets
open Arbitration.Domain.Types
    
type PriceQuery = Query<AssetId, PriceResult>

type SpreadQuery = Query<AssetSpreadId, SpreadResult>