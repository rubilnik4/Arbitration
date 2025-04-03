module Arbitration.Infrastructure.Caches.MarketCacheKeys


[<Literal>]
let LastPricePrefix = "LastPrice"

[<Literal>]
let LastSpreadPrefix = "LastSpread"

let getKey prefix key =
    prefix + "_" + key