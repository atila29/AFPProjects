module Platform.App

open System
open System.IO
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Cors.Infrastructure
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Giraffe
open Platform.Handlers
open Platform.DBSeed

// ---------------------------------
// Web app
// ---------------------------------

let webApp =
    choose [
        GET >=>
            choose [
                route "/" >=> index () 
                route "/teacher"  >=> teacherView
                route "/head" >=> headOfTeacherGetHandler
                route "/student" >=> studentGetHandler
            ]
        POST >=> 
            choose [
                route "/api/student" >=> addStudentHandler >=> redirectTo true "/head"
                route "/api/group/create" >=> createGroupHandler >=> redirectTo true "/teacher"
                route "/api/group/add" >=> addStudentToGroupHandler >=> redirectTo true "/teacher"
                route "/api/group/priority" >=> setGroupPriority >=> redirectTo true "/student"
                route "/api/project/submit" >=> submitProjectProposalsHandler >=> redirectTo true "/teacher"
                route "/api/project/accept" >=> acceptProjectProposal >=> redirectTo true "/head"
                route "/api/project/decline" >=> declineProjectProposal >=> redirectTo true "/head"
                route "/api/project/publish" >=> publishProject >=> redirectTo true "/head"
                route "/api/project/assign" >=> assignProjectToGroupHandler >=> redirectTo true "/teacher"
            ]        
        setStatusCode 404 >=> text "Not Found" ]

// ---------------------------------
// Error handler
// ---------------------------------

let errorHandler (ex : Exception) (logger : ILogger) =
    logger.LogError(ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500 >=> text ex.Message

// ---------------------------------
// Config and Main
// ---------------------------------

let configureCors (builder : CorsPolicyBuilder) =
    builder.WithOrigins("http://localhost:8080")
           .AllowAnyMethod()
           .AllowAnyHeader()
           |> ignore

let configureApp (app : IApplicationBuilder) =
    let env = app.ApplicationServices.GetService<IHostingEnvironment>()
    (match env.IsDevelopment() with
    | true  -> app.UseDeveloperExceptionPage()
    | false -> app.UseGiraffeErrorHandler errorHandler)
        .UseHttpsRedirection()
        .UseCors(configureCors)
        .UseStaticFiles()
        .UseGiraffe(webApp)

let configureServices (services : IServiceCollection) =
    services.AddCors()    |> ignore
    services.AddGiraffe() |> ignore

let configureLogging (builder : ILoggingBuilder) =
    builder.AddFilter(fun l -> l.Equals LogLevel.Error)
           .AddConsole()
           .AddDebug() |> ignore

[<EntryPoint>]
let main _ =
    seedDB |> ignore
    let contentRoot = Directory.GetCurrentDirectory()
    let webRoot     = Path.Combine(contentRoot, "WebRoot")
    WebHostBuilder()
        .UseKestrel()
        .UseContentRoot(contentRoot)
        .UseIISIntegration()
        .UseWebRoot(webRoot)
        .Configure(Action<IApplicationBuilder> configureApp)
        .ConfigureServices(configureServices)
        .ConfigureLogging(configureLogging)
        .Build()
        .Run()
    0