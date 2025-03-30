module Arbitration.Infrastructure.MarketData

open Arbitration.Application.Interfaces

let private getPrice env asset =
    let spreadApi = env.SpreadApi
    spreadApi.GetPrice env asset

let private getLastPrice env asset =
    let spreadRepository = env.SpreadRepository
    spreadRepository.GetLastPrice env.Postgres asset
    
let private getLastSpread env spreadAsset =
    let spreadRepository = env.SpreadRepository
    spreadRepository.GetLastSpread env.Postgres spreadAsset

let MarketData : MarketData = {
    GetPrice = getPrice
    GetLastPrice = getLastPrice
    GetLastSpread = getLastSpread
}