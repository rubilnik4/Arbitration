module Arbitration.Infrastructure.MarketRepository

open System
open System.Threading.Tasks
open Arbitration.Application.Interfaces
open Arbitration.Domain.Models.Spreads
open Arbitration.Domain.Models.Prices
open Arbitration.Domain.Models.Assets
open Npgsql.FSharp
 
let private tryDb (action: unit -> Task<'a>)  : Task<Result<'a, string>> = task {
    try
        let! result = action()
        return Ok result
    with ex ->
        return Error $"DB error: {ex.Message}"
}

let private insertPrice id price =
    """
    INSERT INTO prices (id, asset, price, time)
    SELECT @id, @asset, @price, @time
    WHERE NOT EXISTS (
        SELECT 1 FROM prices WHERE asset = @asset AND time = @time
    )
    """,
    [
        [
            "@id", Sql.uuid id
            "@asset", Sql.string price.Asset
            "@price", Sql.decimal price.Value
            "@time", Sql.timestamp price.Time
        ]
    ]
    
let saveSpread env spread = task {
    let priceAId = Guid.NewGuid()
    let priceBId = Guid.NewGuid()
    let spreadId = Guid.NewGuid()

    let parameters = [
        insertPrice priceAId spread.PriceA
        insertPrice priceBId spread.PriceB

        """
        INSERT INTO spreads (id, price_a_id, price_b_id, spread_value, spread_time)
        VALUES (@id, @price_a_id, @price_b_id, @spread_value, @spread_time)
        """,
        [
            [
                "@id", Sql.uuid spreadId
                "@price_a_id", Sql.uuid priceAId
                "@price_b_id", Sql.uuid priceBId
                "@spread_value", Sql.decimal spread.Value
                "@spread_time", Sql.timestamp spread.Time
            ]
        ]
    ]

    let! _ =
        env.Source.ConnectionString
        |> Sql.connect
        |> Sql.executeTransactionAsync parameters

    return spreadId
}

let getLastPrice env assetId = task {
    let! result =
        Sql.connect env.Source.ConnectionString
        |> Sql.query """
            SELECT id, asset, price, time
            FROM prices
            WHERE asset = @asset
            ORDER BY time DESC
            LIMIT 1
        """
        |> Sql.parameters [ "@asset", Sql.string assetId ]
        |> Sql.executeAsync (fun read ->
            {
                Asset = read.string "asset"
                Value = read.decimal "price"
                Time = read.dateTime "time"
            }
        )

    match result with    
    | head::_ -> return Ok head
    | [] -> return Error $"Database price for asset '{assetId}' not found"
}

let getLastSpread env spreadAssetId = task {
    let assetIdA, assetIdB = spreadAssetId
    let! result =
        Sql.connect env.Source.ConnectionString
        |> Sql.query """
            SELECT s.spread_value, s.spread_time,
                   pa.asset AS asset_a, pa.price AS price_a, pa.time AS time_a,
                   pb.asset AS asset_b, pb.price AS price_b, pb.time AS time_b
            FROM spreads s
            JOIN prices pa ON s.price_a_id = pa.id
            JOIN prices pb ON s.price_b_id = pb.id
            WHERE pa.asset = @asset_a AND pb.asset = @asset_b
            ORDER BY s.spread_time DESC
            LIMIT 1
        """
        |> Sql.parameters [
            "@asset_a", Sql.string assetIdA
            "@asset_b", Sql.string assetIdB
        ]
        |> Sql.executeAsync (fun read ->
            {
                Value = read.decimal "spread_value"
                Time = read.dateTime "spread_time"
                PriceA = {
                    Asset = read.string "asset_a"
                    Value = read.decimal "price_a"
                    Time = read.dateTime "time_a"
                }
                PriceB = {
                    Asset = read.string "asset_b"
                    Value = read.decimal "price_b"
                    Time = read.dateTime "time_b"
                }
            }
        )

    match result with    
    | head::_ -> return Ok head
    | [] -> return Error $"Spread for assets '{assetIdA}' and '{assetIdB}' not found"
}

let postgresSpreadRepository : MarketRepository = {
    SaveSpread =
        fun env spread ->
            tryDb(fun () -> saveSpread env spread)
    GetLastPrice =
        fun env asset -> task {
            let! result = tryDb(fun () -> (getLastPrice env asset))
            return result |> Result.bind id
        }
    GetLastSpread =
        fun env spreadAsset -> task {
            let! result = tryDb(fun () -> (getLastSpread env spreadAsset))
            return result |> Result.bind id
        }
}