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
open MongoDB.Bson
open MongoDB.Bson


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
                                            courseno = 0;
                                            status = ProjectStatus.Request
                                          })
                                            
                                  ctx.WriteJsonAsync result
                  | Error err -> (RequestErrors.BAD_REQUEST err) next ctx
                )
        }

let acceptProjectProposal: HttpHandler = fun (next : HttpFunc) (ctx : HttpContext) ->
  task {
            // Binds a form payload to a Car object
            let! result = ctx.TryBindFormAsync<AnswerProposalInput>()

            return!
                (match result with
                  | Ok project -> let filter = Builders<ProjectData>.Filter.Eq((fun x -> x.id), BsonObjectId(ObjectId(project.id)))
                                  let update = Builders<ProjectData>.Update.Set((fun x -> x.courseno), project.courseNo.Value).Set((fun x -> x.status), ProjectStatus.Accepted)
                                  ignore(projectsCollection.UpdateOne(filter, update))
                                  ctx.WriteJsonAsync project
                  | Error err -> (RequestErrors.BAD_REQUEST err) next ctx
                )
        }

let declineProjectProposal: HttpHandler = fun (next : HttpFunc) (ctx : HttpContext) ->
  task {
            // Binds a form payload to a Car object
            let! result = ctx.TryBindFormAsync<AnswerProposalInput>()

            return!
                (match result with
                  | Ok project -> let filter = Builders<ProjectData>.Filter.Eq((fun x -> x.id), BsonObjectId(ObjectId(project.id)))
                                  let update = Builders<ProjectData>.Update.Set((fun x -> x.status), ProjectStatus.Declined)
                                  ignore(projectsCollection.UpdateOne(filter, update))
                                  ctx.WriteJsonAsync project
                  | Error err -> (RequestErrors.BAD_REQUEST err) next ctx
                )
        }

        

let teacherView () = htmlView ( [
    teacherTemplate students
] |> layout)

let studentGetHandler: HttpHandler = fun (next : HttpFunc) (ctx : HttpContext) -> task {

        let publishedProjects = readAll
                                |> List.ofSeq
                                |> List.map (fun r -> {
                                    ProjectData.id = r.id;
                                    ProjectData.title = r.title;
                                    ProjectData.description = r.description;
                                    ProjectData.teacher = r.teacher;
                                    ProjectData.courseno = r.courseno;
                                    ProjectData.status = r.status;
                                }) 
                                |> List.filter (fun pd -> pd.status = ProjectStatus.Published)
        return! ctx.WriteHtmlViewAsync ( [
            (inspectPublishedProjectsView publishedProjects)
          ] |> layout) 
}


let index () = htmlView ([] |> layout)


