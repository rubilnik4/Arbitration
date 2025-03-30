module Arbitration.Application.Queries.Queries

open System.Threading.Tasks
open Arbitration.Application.Interfaces

open Arbitration.Domain.Models
open Arbitration.Domain.Types
    
type PriceQuery = ArbitrationQuery<Env, AssetId, PriceResult>

type SpreadQuery = ArbitrationQuery<Env, SpreadAsset, SpreadResult>