module Arbitration.Infrastructure.SpreadRepository

open System
open System.Threading.Tasks
open Arbitration.Application.Environments
open Arbitration.Application.Interfaces
open Arbitration.Domain.Models
open Arbitration.Domain.Types
open Npgsql
open Npgsql.FSharp

let private getId<'T>  (insert : 'T) (ids: Guid list) : Guid =
    match ids with
        | id :: _ ->
            id 
        | [] ->
            failwith $"No ID returned from INSERT {insert}"
            
let private saveSpread (env: PostgresEnv) (spread: Spread) : Task<SpreadId> = task {
    let! idResult =
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
        |> Sql.executeAsync (fun read -> read.uuid "id")
        
    return idResult |> getId spread
}

let private saveSpread (env: PostgresEnv) (spread: Spread) : Task<Result<unit, string>> = task {
    try
        do!
        return Ok ()
    with ex ->
        return Error $"Failed to save spread: {ex.Message}"
}

let private getLastPrice(symbol: string, time: DateTimeOffset) : Task<Option<decimal>> = task {
    use! conn = connectionFactory()
    let sql = """
        SELECT source_a_price AS Price FROM spreads 
        WHERE symbol_a = @Symbol 
        AND spread_time <= @Time 
        ORDER BY spread_time DESC 
        LIMIT 1
    """
    let! result = conn.QueryFirstOrDefaultAsync<Nullable<decimal>>(sql, {| Symbol = symbol; Time = time |})
    return Option.ofNullable result
}

let postgresSpreadRepository : SpreadRepository = {
    SaveSpread = getPrice
    GetLastPrice = getLastPrice
}