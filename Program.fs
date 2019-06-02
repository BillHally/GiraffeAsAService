module GiraffeAsAService.App

open System
open System.IO
open System.Collections.Generic
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Hosting.WindowsServices
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection

open Giraffe
open Giraffe.Razor
open Giraffe.Razor.HttpHandlers
open Giraffe.Razor.Middleware
open GiraffeAsAService.Models

// ---------------------------------
// Web app
// ---------------------------------

let webApp =
    choose [
        GET >=>
            choose [
                route "/" >=> razorHtmlView "Index" (Some { Text = "Hello world, from Giraffe (running as a service)!" } ) None None
            ]
        setStatusCode 404 >=> text "Not Found" ]

// ---------------------------------
// Error handler
// ---------------------------------

let errorHandler (ex : Exception) (logger : ILogger) =
    logger.LogError(EventId(), ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500 >=> text ex.Message

// ---------------------------------
// Config and Main
// ---------------------------------

let configureApp (app : IApplicationBuilder) =
    app.UseGiraffeErrorHandler(errorHandler)
       .UseStaticFiles()
       .UseGiraffe webApp

let configureServices (services : IServiceCollection) =
    let sp  = services.BuildServiceProvider()
    let env = sp.GetService<IHostingEnvironment>()
    let viewsFolderPath = Path.Combine(env.ContentRootPath, "Views")
    services.AddRazorEngine viewsFolderPath |> ignore

let configureLogging (builder : ILoggingBuilder) =
    let filter (l : LogLevel) = l.Equals LogLevel.Error
    builder.AddFilter(filter).AddConsole().AddDebug() |> ignore

[<EntryPoint>]
let main argv =
    let pathToAssembly = System.Reflection.Assembly.GetExecutingAssembly().Location
    let contentRoot = Path.GetDirectoryName(pathToAssembly)
    let webRoot     = Path.Combine(contentRoot, "WebRoot")
    
    let host =
        WebHostBuilder()
            .UseKestrel()
            .UseContentRoot(contentRoot)
            .UseIISIntegration()
            .UseWebRoot(webRoot)
            .Configure(Action<IApplicationBuilder> configureApp)
            .ConfigureServices(configureServices)
            .ConfigureLogging(configureLogging)
            .Build()

    if Environment.UserInteractive then
        host.Run()
    else
        host.RunAsService()
    
    0
    