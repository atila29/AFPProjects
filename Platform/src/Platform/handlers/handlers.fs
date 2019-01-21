module Platform.Handlers

open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks.V2.ContextInsensitive
open Giraffe;
open Platform.Model

let submitRequestHandler: HttpHandler = fun (next : HttpFunc) (ctx : HttpContext) ->
  task {
            // Binds a form payload to a Car object
            let! result = ctx.TryBindFormAsync<Request>()

            return!
                (match result with
                | Ok request -> ctx.WriteJsonAsync result
                | Error err -> (RequestErrors.BAD_REQUEST err) next ctx
                )
        }