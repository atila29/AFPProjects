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
open MongoDB.Bson.Serialization.IdGenerators
open MongoDB.Bson


[<Literal>]
let DbName = "platformdb"
let client = MongoClient()
let db = client.GetDatabase(DbName)

let projectsCollection = db.GetCollection<Project>("projects") // maybe projects?
let studentsCollection = db.GetCollection<Student>("students")
let groupsCollection = db.GetCollection<Group>("groups");
let teachersCollection = db.GetCollection<Teacher>("teachers")
let headsOfStudyCollection = db.GetCollection<HeadOfStudy>("headsOfStudy")

let create ( request : Project ) = 
  projectsCollection.InsertOne( request )
let readAll =
  projectsCollection.Find(Builders.Filter.Empty).ToEnumerable()

let students = studentsCollection
                    .Find(Builders.Filter.Empty)
                    .ToEnumerable()

let teachers = teachersCollection
                    .Find(Builders.Filter.Empty)
                    .ToEnumerable()
                    |> List.ofSeq
                    |> List.map (fun t -> t.email)

let headsOfStudy = headsOfStudyCollection
                        .Find(Builders.Filter.Empty)
                        .ToEnumerable()
                        |> List.ofSeq
                        |> List.map (fun h -> h.email)

let groups = groupsCollection
                  .Find(Builders.Filter.Empty)
                  .ToEnumerable()

let headOfTeacherGetHandler: HttpHandler = fun (next : HttpFunc) (ctx : HttpContext) -> task {

  let projects = readAll
                  |> List.ofSeq

  return! ctx.WriteHtmlViewAsync ( [
    headOfStudyView projects (students |> List.ofSeq)
    ] |> layout) 
}

let createGroupHandler: HttpHandler = fun (next : HttpFunc) (ctx : HttpContext) ->
  task {
          let! result = ctx.TryBindFormAsync<GroupInput>()

          return!
              (match result with
              | Ok group -> ignore(groupsCollection.InsertOneAsync({
                                    id=ObjectId.GenerateNewId();
                                    number=group.number;
                                    students=List.Empty;
                                    projectId=ObjectId.Empty
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
              | Ok input -> let student = studentsCollection.Find(Builders<Student>.Filter.Eq((fun x -> x.studynumber), input.studynumber)).First()
                            let groupfilter = Builders<Group>.Filter.Eq((fun x -> x.number), input.groupNumber)
                            let update = Builders<Group>.Update.AddToSet((fun x -> x.students), student)
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
                | Ok student -> if (studentsCollection.Find(Builders.Filter.Empty).ToEnumerable() 
                                    |> Seq.exists (fun s -> s.studynumber = student.studynumber))
                                then (RequestErrors.BAD_REQUEST "studynumber already exists") next ctx
                                else ignore(studentsCollection.InsertOneAsync({
                                        Student.id=ObjectId.GenerateNewId();
                                        Student.name=student.name;
                                        Student.studynumber=student.studynumber;
                                      }))
                                     ctx.WriteJsonAsync student
                | Error err ->  (RequestErrors.BAD_REQUEST err) next ctx
                )
        }

let submitRequestHandler: HttpHandler = fun (next : HttpFunc) (ctx : HttpContext) ->
  task {
            let! result = ctx.TryBindFormAsync<ProjectProposal>()

            return!
                (match result with
                  | Ok request -> try 
                                      let supervisor =
                                          try
                                               teachersCollection
                                                    .Find(Builders<Teacher>.Filter.Eq((fun t -> t.email), request.teacherEmail))
                                                    .Single()
                                          with
                                          | _ -> failwith "more than one teacher with specified email"
                                      let splitCS (s : string) = s.Split(',') |> List.ofArray |> List.map (fun e -> e.Trim())
                                      let cosupervisorEmails = splitCS request.cosupervisorsEmailCS
                                      let cosupervisors = teachersCollection.Find(Builders<Teacher>.Filter.Where(fun t -> List.contains t.email cosupervisorEmails)).ToEnumerable() |> List.ofSeq

                                      let prerequisites = splitCS request.prerequisitesCS
                                      let restrictions = splitCS request.restrictionsCS |> List.map (fun r -> {name=r; n=None})
                                  
                                      create ({
                                               id=ObjectId.GenerateNewId();
                                               title = request.title;
                                               description = request.description;
                                               teacher = supervisor;
                                               courseno = 0;
                                               status = ProjectStatus.Request;
                                               restrictions = restrictions;
                                               prerequisites = prerequisites;
                                               cosupervisors = cosupervisors;
                                      })    
                                      ctx.WriteJsonAsync result
                                  with
                                  | Failure f -> (RequestErrors.BAD_REQUEST f) next ctx
                  | Error err -> (RequestErrors.BAD_REQUEST err) next ctx
                )
        }

let acceptProjectProposal: HttpHandler = fun (next : HttpFunc) (ctx : HttpContext) ->
  task {
            let! result = ctx.TryBindFormAsync<AnswerProposalInput>()

            return!
                (match result with
                  | Ok project -> let filter = Builders<Project>.Filter.Eq((fun x -> x.id), ObjectId(project.id))
                                  let update = Builders<Project>.Update.Set((fun x -> x.courseno), project.courseNo.Value).Set((fun x -> x.status), ProjectStatus.Accepted)
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
                  | Ok project -> let filter = Builders<Project>.Filter.Eq((fun x -> x.id), ObjectId(project.id))
                                  let update = Builders<Project>.Update.Set((fun x -> x.status), ProjectStatus.Declined)
                                  ignore(projectsCollection.UpdateOne(filter, update))
                                  ctx.WriteJsonAsync project
                  | Error err -> (RequestErrors.BAD_REQUEST err) next ctx
                )
        }

let assignProjectToGroupHandler: HttpHandler = fun (next : HttpFunc) (ctx : HttpContext) -> task {
  let! result = ctx.TryBindFormAsync<ProjectGroupInput>()
  
  return!
    (match result with
      | Ok input -> let filter = Builders<Project>.Filter.Eq((fun x -> x.id), ObjectId(input.projectId))

                    let projectExist = projectsCollection.Find(filter).Any()
                    if (not projectExist) then failwith "Project doesn't exist!"

                    // let student = studentsCollection.Find(Builders<Student>.Filter.Eq((fun x -> x.studynumber), input.studynumber)).First()
                    let groupfilter = Builders<Group>.Filter.Eq((fun x -> x.number), input.groupNumber)
                    let update = Builders<Group>.Update.Set((fun x -> x.projectId), ObjectId(input.projectId))

                    groupsCollection.UpdateOne(groupfilter, update) |> ignore

                    ctx.WriteJsonAsync input

      | Error err -> (RequestErrors.BAD_REQUEST err) next ctx
    )
}

let teacherView: HttpHandler = fun (next : HttpFunc) (ctx : HttpContext) -> task {
  return! ctx.WriteHtmlViewAsync ( [
    teacherTemplate (students |> List.ofSeq) (groups |> List.ofSeq) // Jeg slettede noget her under merge, ved ikke om det er korrekt?
    ] |> layout) 
}


let publishProjectProposal: HttpHandler = fun (next : HttpFunc) (ctx : HttpContext) ->
  task {
        let! result = ctx.TryBindFormAsync<AnswerProposalInput>()

        return!
            (match result with
              | Ok project -> let filter = Builders<Project>.Filter.Where(fun p -> p.id = ObjectId(project.id) && p.status = ProjectStatus.Accepted)
                              let update = Builders<Project>.Update.Set((fun x -> x.status), ProjectStatus.Published)
                              let updateResult = projectsCollection.UpdateOne(filter, update)
                              match updateResult.MatchedCount with
                                | 0L -> (RequestErrors.BAD_REQUEST "Project doesn't exist or hasn't been accepted") next ctx
                                | n -> ctx.WriteJsonAsync project
              | Error err -> (RequestErrors.BAD_REQUEST err) next ctx
            )
}
   


let studentGetHandler: HttpHandler = fun (next : HttpFunc) (ctx : HttpContext) -> task {

        let publishedProjects = readAll
                                |> List.ofSeq
                                |> List.map (fun r -> {
                                    Project.id = r.id;
                                    Project.title = r.title;
                                    Project.description = r.description;
                                    Project.teacher = r.teacher;
                                    Project.courseno = r.courseno;
                                    Project.status = r.status;
                                    Project.cosupervisors = r.cosupervisors;
                                    Project.prerequisites = r.prerequisites;
                                    Project.restrictions = r.restrictions;
                                }) 
                                |> List.filter (fun pd -> pd.status = ProjectStatus.Published)
        return! ctx.WriteHtmlViewAsync ( [
            (inspectPublishedProjectsView publishedProjects)
          ] |> layout) 
}


let index () = htmlView ([] |> layout)


