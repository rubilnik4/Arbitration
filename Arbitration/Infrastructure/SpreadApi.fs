module Arbitration.Infrastructure.SpreadApi

open Arbitration.Application.Interfaces
open Arbitration.Domain.Models.Prices
   
let private getPrice env asset = task {
    try       
        let client = env.BinanceRestClient
        let! result = client.UsdFuturesApi.ExchangeData.GetPriceAsync asset
        match result.Success with
        | true ->
            let price = {
                Asset = asset
                Value = result.Data.Price
                Time = result.Data.Timestamp |> DateTimeUtils.withUtcDatetime
            }
            return price |> Ok            
        | false ->
            return $"Binance error: {result.Error.Message}" |> Error
    with ex ->
        return $"Binance API error: {ex.Message}" |> Error
}

let binanceSpreadApi : SpreadApi = {
    GetPrice = getPrice
}