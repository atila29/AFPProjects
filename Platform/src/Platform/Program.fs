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
open Platform.HtmlViews

// ---------------------------------
// Models
// ---------------------------------

type Message =
    {
        Text : string
    }

// ---------------------------------
// Views
// ---------------------------------

module Views =
    open GiraffeViewEngine

    let layout (content: XmlNode list) =
        html [] [
            head [] [
                title []  [ encodedText "Platform" ]
                link [ _rel  "stylesheet"
                       _type "text/css"
                       _href "/main.css" ]
                link [ _rel  "stylesheet"
                       _href "https://fonts.googleapis.com/css?family=Roboto:300,400,500,700|Material+Icons" ]
                link [ _rel  "stylesheet"
                       _href "https://unpkg.com/bootstrap-material-design@4.1.1/dist/css/bootstrap-material-design.min.css" ]
                       //<link rel="stylesheet" href="https://fonts.googleapis.com/css?family=Roboto:300,400,500,700|Material+Icons">
                        //<link rel="stylesheet" href="https://unpkg.com/bootstrap-material-design@4.1.1/dist/css/bootstrap-material-design.min.css" integrity="sha384-wXznGJNEXNG1NFsbm0ugrLFMQPWswR3lds2VeinahP8N0zJw9VWSopbjv2x7WCvX" crossorigin="anonymous">


            ]
            body [] content
        ]

    let navbar () = ([
        ul [_class "nav nav-tabs bg-primary"] [
            li [_class "nav-item"] [
                a [ _class "nav-link active"; _href "/" ] [ str "index"]
            ]
            li [_class "nav-item"] [
                a [ _class "nav-link active"; _href "/student" ] [ str "student" ]
            ]
            li [_class "nav-item"] [
                a [ _class "nav-link active"; _href "/teacher" ] [ str "teacher" ]
            ]
            li [_class "nav-item"] [
                a [ _class "nav-link active"; _href "/head" ] [ str "head of study" ]
            ]
        ]
    ] |> layout)
//     <!-- primary -->
// <ul class="nav nav-tabs bg-primary">
//   <li class="nav-item">
//     <a class="nav-link active" href="#">Active</a>
//   </li>
//   <li class="nav-item">
//     <a class="nav-link" href="#">Link</a>
//   </li>
//   <li class="nav-item">
//     <a class="nav-link" href="#">Another link</a>
//   </li>
//   <li class="nav-item">
//     <a class="nav-link disabled" href="#">Disabled</a>
//   </li>
// </ul>

    let partial () = ([
        navbar()
        h1 [] [ encodedText "Platform" ]
    ] |> layout)

    let teacherView = ([
        partial()
        teacherTemplate()
    ] |> layout)
    
    let headOfStudyView = ([
        partial()
        headOfStudyTemplate()
    ] |> layout)

    let index (model : Message) =
        [
            partial()
        ] |> layout

// ---------------------------------
// Web app
// ---------------------------------

let indexHandler (name : string) =
    let greetings = sprintf "Hello %s, from Giraffe!" name
    let model     = { Text = greetings }
    let view      = Views.index model
    htmlView view

let webApp =
    choose [
        GET >=>
            choose [
                route "/" >=> indexHandler "world"
                route "/teacher"  >=> htmlView Views.teacherView
                route "/head" >=> htmlView Views.headOfStudyView
            ]
        POST >=> 
            choose [
                route "/submitrequest" >=> submitRequestHandler
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