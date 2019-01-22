module Platform.Handlers


open System;
open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks.V2.ContextInsensitive
open Giraffe;
open Platform.Model
open FSharp.Data.Sql


[<Literal>]
let connStr = "User ID=postgres;Host=localhost;Port=5432;Database=projectframework;Password=1234"

type sql = SqlDataProvider<Common.DatabaseProviderTypes.POSTGRESQL, connStr>


let dbcontext = sql.GetDataContext()


let submitRequestHandler: HttpHandler = fun (next : HttpFunc) (ctx : HttpContext) ->
  task {
            // Binds a form payload to a Car object
            let! result = ctx.TryBindFormAsync<Request>()
            
            //let projectTable = dbcontext.Public.Project.``Create(desc, teacher, title)``

            let p = dbcontext.Public.Project

            return!
                (match result with
                | Ok request -> ctx.WriteJsonAsync result
                | Error err -> (RequestErrors.BAD_REQUEST err) next ctx
                )
        }