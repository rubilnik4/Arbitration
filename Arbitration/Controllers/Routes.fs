module Arbitration.Controllers.Routes

[<Literal>]
let Api = "api"

[<Literal>]
let PriceRoute = "price"

[<Literal>]
let SpreadRoute = "spread"

let getRoute route = $"{Api}/{route}"