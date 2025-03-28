module Arbitration.Infrastructure.MarketData

open System
open System.Net.Http
open System.Threading.Tasks
open Arbitration.Application.Interfaces
open Arbitration.Domain.Models
open Arbitration.Domain.Types
open Binance.Net.Clients
open Binance.Net.Objects.Options

let private getPrice (asset: AssetId) : Task<PriceResult> = task {
    try       
        use client = new BinanceRestClient()
        let! result = client.UsdFuturesApi.ExchangeData.GetPriceAsync asset
        match result.Success with
        | true ->
            let price = {
                Asset = asset
                Value = result.Data.Price
                Time = result.Data.Timestamp |> TimerService.withUtcDatetime
            }
            return price |> Ok            
        | false ->
            return $"Binance error: {result.Error.Message}" |> Error
    with ex ->
        return $"Binance API error: {ex.Message}" |> Error
}

let private getLastPrice (asset: AssetId) : Task<PriceResult> = task {
    return Error "Not implemented"
}

let binanceMarketData : MarketData = {
    GetPrice = getPrice
    GetLastPrice = getLastPrice
}