module Platform.Handlers

open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks.V2.ContextInsensitive
open Giraffe;
open Platform.Model.Input
open Platform.Model.Data
open Platform.HtmlViews

open MongoDB.Bson
open MongoDB.Driver
open MongoDB.FSharp
open Platform.Model


[<Literal>]
let DbName = "platformdb"
let client = MongoClient()
let db = client.GetDatabase(DbName)
let requestCollection = db.GetCollection<RequestData>("request") // maybe projects?


let create ( request : RequestData ) = 
  requestCollection.InsertOne( request )
let readAll =
  requestCollection.Find(Builders.Filter.Empty).ToEnumerable()

let headOfTeacherGetHandler: HttpHandler = fun (next : HttpFunc) (ctx : HttpContext) -> task {


  let x = readAll
          |> List.ofSeq
          |> List.map (fun r -> {
            Request.description= r.description;
            Request.id= r.id;
            Request.teacher= r.teacher;
            Request.title= r.title;
          })

  // (headOfStudyTemplate ( |> List.map (fun r -> r)))

  return! ctx.WriteHtmlViewAsync ( [
    (headOfStudyTemplate x)
    ] |> layout) 
}



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

let teacherView () = htmlView ( [
    teacherTemplate()
] |> layout)

let index () = htmlView ([] |> layout)


// let headOfStudyView = 
// ([
//     let x =headOfTeacherGetHandler()
    
// ] |> layout)

