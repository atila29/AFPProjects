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
let groupsCollection = db.GetCollection<GroupData>("groups");

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

let createGroupHandler: HttpHandler = fun (next : HttpFunc) (ctx : HttpContext) ->
  task {
          let! result = ctx.TryBindFormAsync<GroupInput>()

          return!
              (match result with
              | Ok group -> ignore(groupsCollection.InsertOneAsync({
                                    id=ObjectId.GenerateNewId();
                                    name=group.name;
                                    students=List.Empty;
                                  }))
                            ctx.WriteJsonAsync group
              | Error err -> (RequestErrors.BAD_REQUEST err) next ctx
              )
        }

let addStudentToGroupHandler: HttpHandler = fun (next : HttpFunc) (ctx : HttpContext) ->
  task {
          let! result = ctx.TryBindFormAsync<StudentGroupInput>()

          return!
              (match result with
              | Ok input -> let student = studentsCollection.Find(Builders<StudentData>.Filter.Eq((fun x -> x.id), input.studentId)).First()
                            let groupfilter = Builders<GroupData>.Filter.Eq((fun x -> x.id), ObjectId(input.id))
                            let update = Builders<GroupData>.Update.AddToSet((fun x -> x.students), student)
                            
                            ignore(groupsCollection.UpdateOne(groupfilter, update))
                            ctx.WriteJsonAsync input
              | Error err -> (RequestErrors.BAD_REQUEST err) next ctx
              )
        }      

let addStudentHandler: HttpHandler = fun (next : HttpFunc) (ctx : HttpContext) ->
  task {
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
            let! result = ctx.TryBindFormAsync<ProjectProposal>()

            return!
                (match result with
                  | Ok request -> create ({
                                            id=ObjectId.GenerateNewId();
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
            let! result = ctx.TryBindFormAsync<AnswerProposalInput>()

            return!
                (match result with
                  | Ok project -> let filter = Builders<ProjectData>.Filter.Eq((fun x -> x.id), ObjectId(project.id))
                                  let update = Builders<ProjectData>.Update.Set((fun x -> x.courseno), project.courseNo.Value).Set((fun x -> x.status), ProjectStatus.Accepted)
                                  ignore(projectsCollection.UpdateOne(filter, update))
                                  ctx.WriteJsonAsync project
                  | Error err -> (RequestErrors.BAD_REQUEST err) next ctx
                )
        }

let declineProjectProposal: HttpHandler = fun (next : HttpFunc) (ctx : HttpContext) ->
  task {
            let! result = ctx.TryBindFormAsync<AnswerProposalInput>()

            return!
                (match result with
                  | Ok project -> let filter = Builders<ProjectData>.Filter.Eq((fun x -> x.id), ObjectId(project.id))
                                  let update = Builders<ProjectData>.Update.Set((fun x -> x.status), ProjectStatus.Declined)
                                  ignore(projectsCollection.UpdateOne(filter, update))
                                  ctx.WriteJsonAsync project
                  | Error err -> (RequestErrors.BAD_REQUEST err) next ctx
                )
        }

        

let teacherView () = htmlView ( [
    teacherTemplate students
] |> layout)

let index () = htmlView ([] |> layout)


