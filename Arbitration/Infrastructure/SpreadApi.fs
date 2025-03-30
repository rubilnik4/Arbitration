module Arbitration.Infrastructure.SpreadApi

open Arbitration.Application.Interfaces
open Binance.Net.Clients
open Arbitration.Domain.Models
open Binance.Net.Objects.Options
open Microsoft.Extensions.Options

let private getClient env =
    let options = BinanceRestOptions()
    let optionsWrapper = Options.Create(options)
    let client = env.HttpClientFactory.CreateClient("binance")
    new BinanceRestClient(client, env.Logger, optionsWrapper)
    
let private getPrice env asset = task {
    try       
        let client = getClient env
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

let BinanceSpreadApi : SpreadApi = {
    GetPrice = getPrice
}