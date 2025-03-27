module Arbitration.Domain.Types

type Asset = string

type SpreadId = string

type SpreadQuery =
    | GetLastSpread
    | GetHistory

type SpreadCommand =
    | ComputeSpread of Asset * Asset