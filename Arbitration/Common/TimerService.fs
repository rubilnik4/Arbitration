module Arbitration.Infrastructure.TimerService

open System

let getUtcDatetime() =
    DateTime.UtcNow
    
let withUtcDatetime (time: Nullable<DateTime>) =
    time
    |> Option.ofNullable
    |> Option.defaultValue getUtcDatetime() 
