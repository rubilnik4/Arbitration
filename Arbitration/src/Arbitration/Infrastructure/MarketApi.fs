module Arbitration.Infrastructure.MarketApi

open System
open Arbitration.Application.Interfaces
open Arbitration.Domain.Models.Assets
open Arbitration.Domain.Models.Prices
open Arbitration.Domain.DomainTypes
open Arbitration.Shared.Core.Option
open Microsoft.Extensions.Logging
open Microsoft.FSharp.Core
    
let private getPrice env (assetId: AssetId) = task {
    try
        env.Logger.LogDebug("Binance API request for asset: {assetId}", assetId)
        
        let client = env.BinanceRestClient
        let! result = client.UsdFuturesApi.ExchangeData.GetPriceAsync assetId        
        match result.Success with
        | true ->
            let price = {
                Asset = assetId
                Value = result.Data.Price
                Time = result.Data.Timestamp |> toVOption DateTime.UtcNow
            }
            
            env.Logger.LogDebug("Binance API response for {assetId}: {price}", assetId, price)
            return price |> Ok            
        | false ->
            let code = result.Error.Code |> toVOption 0
            env.Logger.LogWarning("Binance API error for {assetId}: Code {errorCode}, Message: {message}",
                                  assetId, code, result.Error.Message)
            return ApiError ($"Binance API error for {assetId}", code, result.Error.Message) |> Error
    with ex ->        
        env.Logger.LogError(ex, "Binance API request failed for {AssetId}", assetId)
        return ServiceUnavailable ($"Binance API request failed for {assetId}", ex) |> Error
}

let binanceMarketApi : MarketApi = {
    GetPrice = getPrice
}