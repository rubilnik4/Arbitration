module Arbitration.Infrastructure.DateTimeUtils

open System

let getUtcDatetime() =
    DateTime.UtcNow
    
let withUtcDatetime (time: Nullable<DateTime>) =
    time
    |> Option.ofNullable
    |> Option.defaultWith (fun _ -> getUtcDatetime())  
