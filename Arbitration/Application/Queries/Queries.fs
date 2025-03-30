module Arbitration.Application.Queries.Queries

open Arbitration.Application.Interfaces
open Arbitration.Domain.Models.Assets
open Arbitration.Domain.Types
    
type PriceQuery = ArbitrationQuery<Env, AssetId, PriceResult>

type SpreadQuery = ArbitrationQuery<Env, AssetSpread, SpreadResult>