module Arbitration.Program

#nowarn "20"
open Arbitration.Application.Interfaces
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.OpenApi.Models
open Oxpecker

let sayHelloWorld : EndpointHandler = text "Hello World, from Oxpecker"

let webApp = GET [
    route "/" <| sayHelloWorld
]
    
let configureApp (appBuilder: IApplicationBuilder) =
    appBuilder
        .UseRouting()
        .UseOxpecker(webApp)
        .UseSwagger()
        .UseSwaggerUI(fun c ->
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Spread API V1")
            //c.RoutePrefix <- ""
        )
    |> ignore

let configureServices (services: IServiceCollection) =
    services
        .AddRouting()
        .AddOxpecker()
        .AddSingleton(createEnv())
        .AddEndpointsApiExplorer()
        .AddSwaggerGen(fun c ->
            c.SwaggerDoc("v1", OpenApiInfo(
                Title = "Spread API",
                Version = "v1",
                Description = "Сервис для вычисления спреда между двумя активами"
            ))            
            //c.CustomSchemaIds _.Name
    )
    |> ignore
    
[<EntryPoint>]
let main args =
    let builder = WebApplication.CreateBuilder(args)
    configureServices builder.Services
    let app = builder.Build()
    configureApp app
    app.Run()
    0