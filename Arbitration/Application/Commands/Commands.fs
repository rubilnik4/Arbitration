module Arbitration.Application.Commands.Commands

open Arbitration.Application.Interfaces
open Arbitration.Domain.Models
open Arbitration.Domain.Types

type SpreadCommand = ArbitrationCommand<Env, SpreadState, SpreadAsset, SpreadResult>

