module Arbitration.Program

#nowarn "20"
open Arbitration.Application.CompositionRoot
open Arbitration.Application.Interfaces
open Arbitration.Controllers.PriceEndpoint
open Arbitration.Controllers.SpreadEndpoint
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.OpenApi.Models
open Oxpecker

   
[<EntryPoint>]
let main args =
    let builder = WebApplication.CreateBuilder(args)
    configureServices builder.Services
    let app = builder.Build()
    configureApp app
    app.Run()
    0