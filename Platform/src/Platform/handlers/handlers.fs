module Platform.Handlers

open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks.V2.ContextInsensitive
open Giraffe;
open Platform.Model.Input
open Platform.Model.Data

open MongoDB.Bson
open MongoDB.Driver
open MongoDB.FSharp
open Platform.Model


[<Literal>]
let DbName = "platformdb"
let client         = MongoClient()
let db             = client.GetDatabase(DbName)
let groupCollection = db.GetCollection<RequestData>("groups")


let create ( request : RequestData ) = 
  groupCollection.InsertOne( request )



let submitRequestHandler: HttpHandler = fun (next : HttpFunc) (ctx : HttpContext) ->
  task {
            // Binds a form payload to a Car object
            let! result = ctx.TryBindFormAsync<Request>()



            return!
                (match result with
                | Ok request -> create ({
                                          id=None;
                                          title = request.title;
                                          description = request.description;
                                          teacher = request.teacher
                                        })
                                          
                                ctx.WriteJsonAsync result
                | Error err -> (RequestErrors.BAD_REQUEST err) next ctx
                )
        }