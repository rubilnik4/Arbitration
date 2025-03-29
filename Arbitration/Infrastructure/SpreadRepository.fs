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

let private savePrice env price = task {
    let! result =
        Sql.connect env.Source.ConnectionString
        |> Sql.query """
            INSERT INTO prices (id, asset, price, time)
            SELECT @id, @asset, @price, @time
            WHERE NOT EXISTS (
                SELECT 1 FROM prices WHERE asset = @asset AND time = @time
            )
            RETURNING id
        """
        |> Sql.parameters [
            "id", Sql.uuid (Guid.NewGuid())
            "asset", Sql.string price.Asset
            "price", Sql.decimal price.Value
            "time", Sql.timestamp price.Time
        ]
        |> Sql.executeAsync (fun read -> read.uuid "id")
        
    return result |> _.Head
}

let private saveSpread (env: PostgresEnv) (spread: Spread) = task {
    let! priceA = savePrice env spread.PriceA
    let! priceB = savePrice env spread.PriceB
    
    let! spread = 
        Sql.connect env.Source.ConnectionString
        |> Sql.query """
            INSERT INTO spreads (
                id, price_a_id, price_b_id, spread_value, spread_time
            ) VALUES (
                @id, @price_a_id, @price_b_id, @spread_value, @spread_time
            )
            RETURNING id
        """
        |> Sql.parameters [
            "id", Sql.uuid (Guid.NewGuid())
            "price_a_id", Sql.uuid priceA
            "price_b_id", Sql.uuid priceA
            "spread_value", Sql.decimal spread.Value
            "spread_time", Sql.timestamp spread.Time
        ]
        |> Sql.executeAsync (fun read -> read.uuid "id")
    
    return spread |> _.Head   
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
    SaveSpread = saveSpread |> tryDb
    GetLastPrice = getLastPrice
}