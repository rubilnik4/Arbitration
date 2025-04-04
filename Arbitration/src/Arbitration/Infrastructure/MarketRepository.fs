module Arbitration.Infrastructure.MarketRepository

open System
open Arbitration.Application.Interfaces
open Arbitration.Domain.Models.Assets
open Arbitration.Domain.Models.Spreads
open Arbitration.Domain.Models.Prices
open Arbitration.Domain.DomainTypes
open Microsoft.Extensions.Logging
open Npgsql.FSharp

let private insertPrice env price = task {
    let priceId = Guid.NewGuid()
     
    let! result =
        env.Postgres
        |> Sql.fromDataSource
        |> Sql.query """
            WITH ins AS (
                INSERT INTO prices (id, asset, price, time)
                VALUES (@id, @asset, @price, @time)
                ON CONFLICT (asset, time) DO NOTHING
                RETURNING id
            )
            SELECT id FROM ins
            UNION ALL
            SELECT id FROM prices 
            WHERE asset = @asset AND time = @time
            LIMIT 1;
        """
        |> Sql.parameters [
            "@id", Sql.uuid priceId
            "@asset", Sql.string price.Asset
            "@price", Sql.decimal price.Value
            "@time", Sql.timestamptz price.Time
        ]
        |> Sql.executeAsync (fun read ->
            read.uuid "id" 
        )
        
    return result.Head 
}

let saveSpread env spread = task {   
    let spreadId = Guid.NewGuid()
    let assetSpreadKey = getSpreadKey spread
        
    let! priceAId = insertPrice env spread.PriceA
    let! priceBId = insertPrice env spread.PriceB 

    let! result =
        env.Postgres
        |> Sql.fromDataSource
        |> Sql.query """
        WITH ins AS (
            INSERT INTO spreads (id, price_a_id, price_b_id, asset_spread_id, spread_value, spread_time)
            VALUES (@id, @price_a_id, @price_b_id, @asset_spread_id, @spread_value, @spread_time)
            ON CONFLICT (asset_spread_id, spread_time) DO NOTHING
            RETURNING id
        )
        SELECT id FROM ins
        UNION ALL
        SELECT id FROM spreads 
        WHERE asset_spread_id = @asset_spread_id AND spread_time = @spread_time
        LIMIT 1;
        """
        |> Sql.parameters [
            "@id", Sql.uuid spreadId
            "@price_a_id", Sql.uuid priceAId
            "@price_b_id", Sql.uuid priceBId
            "asset_spread_id", Sql.string assetSpreadKey
            "@spread_value", Sql.decimal spread.Value
            "@spread_time", Sql.timestamptz spread.Time
        ]
        |> Sql.executeAsync (fun read ->
            read.uuid "id" 
        )

    return result.Head
}

let getLastPrice env assetId = task {
    let! result =
        env.Postgres
        |> Sql.fromDataSource
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
    | head::_ -> return head |> Ok
    | [] -> return NotFound $"Price for asset '{assetId}' not found" |> Error
}

let getLastSpread env assetSpreadId = task {
    let assetSpreadKey = getAssetSpreadKey assetSpreadId
    let! result =
        env.Postgres
        |> Sql.fromDataSource
        |> Sql.query """
            SELECT s.spread_value, s.spread_time,
                   pa.asset AS asset_a, pa.price AS price_a, pa.time AS time_a,
                   pb.asset AS asset_b, pb.price AS price_b, pb.time AS time_b
            FROM spreads s
            JOIN prices pa ON s.price_a_id = pa.id
            JOIN prices pb ON s.price_b_id = pb.id
            WHERE asset_spread_id = @asset_spread_id
            ORDER BY s.spread_time DESC
            LIMIT 1
        """
        |> Sql.parameters [ "@asset_spread_id", Sql.string assetSpreadKey ]
        |> Sql.executeAsync (fun read -> {
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
        })

    match result with    
    | head::_ -> return head |> Ok
    | [] -> return NotFound $"Spread for assets '{assetSpreadKey}' not found" |> Error
}

let postgresSpreadRepository : MarketRepository = {
    SaveSpread =
        fun env spread -> task {
            let spreadKey = getSpreadKey spread
            try                
                env.Logger.LogDebug("Saving spread {spread} to database", spreadKey)
                let! spreadId = saveSpread env spread
                env.Logger.LogDebug("Spread {spread} saved successfully to database", spreadKey)
                return spreadId |> Ok
            with ex ->
                env.Logger.LogError(ex, "Failed to save spread {spread} to database", spreadKey)
                return DatabaseError ($"Failed to save spread {spreadKey}", ex) |> Error
        }          
    GetLastPrice =
        fun env assetId -> task {
            try                
                env.Logger.LogDebug("Getting last price {assetId} from database", assetId)
                let! result = getLastPrice env assetId
                env.Logger.LogDebug("Last price {assetId} get successfully", assetId)
                return result
            with ex ->
                env.Logger.LogError(ex, "Failed to get last price {assetId}", assetId)
                return DatabaseError ($"Failed to get last price {assetId}", ex) |> Error
        }
    GetLastSpread =
        fun env spreadAssetId -> task {          
            try                
                env.Logger.LogDebug("Getting last spread {spreadAssetId} from database", spreadAssetId)
                let! result = getLastSpread env spreadAssetId
                env.Logger.LogDebug("Last spread {spreadAssetId} get successfully", spreadAssetId)
                return result
            with ex ->
                env.Logger.LogError(ex, "Failed to get last spread {spreadAssetId}", spreadAssetId)
                return DatabaseError ($"Failed to get last spread {spreadAssetId}", ex) |> Error
        }
}