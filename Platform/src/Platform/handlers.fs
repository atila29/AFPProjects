module Platform.Handlers

open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks.V2.ContextInsensitive
open Giraffe;
open Platform.Model.Domain
open Platform.Model.Input
open Platform.Model.Data
open Platform.HtmlViews

open MongoDB.Bson
open MongoDB.Driver
open MongoDB.FSharp
open MongoDB.Bson.Serialization.IdGenerators


[<Literal>]
let DbName = "platformdb"
let client = MongoClient()
let db = client.GetDatabase(DbName)
let projectsCollection = db.GetCollection<ProjectData>("projects") // maybe projects?
let studentsCollection = db.GetCollection<StudentData>("students")


let create ( request : ProjectData ) = 
  projectsCollection.InsertOne( request )
let readAll =
  projectsCollection.Find(Builders.Filter.Empty).ToEnumerable()

let students = studentsCollection
                    .Find(Builders.Filter.Empty)
                    .ToEnumerable()
                    |> List.ofSeq
                    |> List.map (fun s -> s.id )

let headOfTeacherGetHandler: HttpHandler = fun (next : HttpFunc) (ctx : HttpContext) -> task {

  let projects = readAll
                  |> List.ofSeq

  

  return! ctx.WriteHtmlViewAsync ( [
    (headOfStudyView projects students)
    ] |> layout) 
}

let addStudentHandler: HttpHandler = fun (next : HttpFunc) (ctx : HttpContext) ->
  task {
            // Binds a form payload to a Car object
            let! result = ctx.TryBindFormAsync<StudentInput>()

            return!
                (match result with
                | Ok student -> ignore(studentsCollection.InsertOneAsync({id=student.id}))
                                ctx.WriteJsonAsync student
                | Error err -> (RequestErrors.BAD_REQUEST err) next ctx
                )
        }

let submitRequestHandler: HttpHandler = fun (next : HttpFunc) (ctx : HttpContext) ->
  task {
            // Binds a form payload to a Car object
            let! result = ctx.TryBindFormAsync<ProjectProposal>()

            return!
                (match result with
                  | Ok request -> create ({
                                            id=BsonObjectId(ObjectId.GenerateNewId());
                                            title = request.title;
                                            description = request.description;
                                            teacher = request.teacher;
                                            courseno = None;
                                          })
                                            
                                  ctx.WriteJsonAsync result
                  | Error err -> (RequestErrors.BAD_REQUEST err) next ctx
                )
        }

let teacherView () = htmlView ( [
    teacherTemplate students
] |> layout)

let index () = htmlView ([] |> layout)


