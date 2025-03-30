module Arbitration.Application.Commands.Commands

open Arbitration.Application.Interfaces
open Arbitration.Domain.Models.Spreads
open Arbitration.Domain.Models.Assets
open Arbitration.Domain.Types

type SpreadCommand = ArbitrationCommand<Env, SpreadState, AssetSpread, SpreadResult>

