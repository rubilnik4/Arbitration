module Arbitration.Infrastructure.SpreadRepository

open System
open System.Threading.Tasks
open Arbitration.Application.Interfaces
open Arbitration.Domain.Models
open Arbitration.Domain.Types
open Npgsql.FSharp
 
let private tryDb (action: unit -> Task<'a>)  : Task<Result<'a, string>> = task {
    try
        let! result = action()
        return Ok result
    with ex ->
        return Error $"DB error: {ex.Message}"
}

let private saveSpread env spread = task {
    let! result = tryDb(fun () ->
        Sql.connect env.Source.ConnectionString
        |> Sql.query """
            INSERT INTO spreads (
                id, asset_a, asset_b, price_a, price_b, time_a, time_b, spread_value, spread_time
            ) VALUES (
                @id, @asset_a, @asset_b, @price_a, @price_b, @time_a, @time_b, @spread_value, @spread_time
            )
            RETURNING id
        """
        |> Sql.parameters [
            "id", Sql.uuid (Guid.NewGuid())
            "asset_a", Sql.string spread.PriceA.Asset
            "asset_b", Sql.string spread.PriceB.Asset
            "price_a", Sql.decimal spread.PriceA.Value
            "price_b", Sql.decimal spread.PriceB.Value
            "time_a", Sql.timestamp spread.PriceA.Time
            "time_b", Sql.timestamp spread.PriceB.Time
            "spread_value", Sql.decimal spread.Value
            "spread_time", Sql.timestamp spread.Time
        ]
        |> Sql.executeAsync (fun read -> read.uuid "id"))
        
    return result |> Result.map _.Head
}

let private getLastPrice env asset = task {
    let! result = tryDb(fun () ->
        Sql.connect env.Source.ConnectionString
        |> Sql.query """
            SELECT 
                asset_a, price_a, 
                asset_b, price_b,
                time_a, time_b
                spread_time
            FROM spreads
            WHERE asset_a = @asset OR asset_b = @asset
            ORDER BY spread_time DESC
            LIMIT 1
        """
        |> Sql.parameters [
            "asset", Sql.string asset
        ]
        |> Sql.executeAsync (fun read -> 
            let assetA = read.string "asset_a"
            let priceA = read.decimal "price_a"
            let assetB = read.string "asset_b"
            let priceB = read.decimal "price_b"
            let timeA = read.dateTime "time_a"
            let timeB = read.dateTime "time_b"
            match assetA, assetB with
            | a, _ when a = asset -> Ok { Asset = a; Value = priceA; Time = timeA }
            | _, b when b = asset -> Ok { Asset = b; Value = priceB; Time = timeB }
            | _ -> Error "price not found"
        ))
   
    return result |> Result.bind _.Head
}

let postgresSpreadRepository : SpreadRepository = {
    SaveSpread = saveSpread
    GetLastPrice = getLastPrice
}