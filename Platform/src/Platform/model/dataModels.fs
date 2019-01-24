module Platform.Model.Data

open MongoDB.Bson
open Newtonsoft.Json.Bson
open System
open MongoDB.Bson


type ProjectStatus = 
  | Request=1
  | Accepted=2
  | Declined=3
  | Published=4

[<CLIMutable>]
type Teacher = {
    id: ObjectId
    name: string
    email: string
}

[<CLIMutable>]
type Student = {
    id: ObjectId
    name: string
    studynumber: string
}

[<CLIMutable>]
type HeadOfStudy = {
    id: ObjectId
    name: string
    department: string
    email: string
}


[<CLIMutable>]
type Project = {
  id: ObjectId
  title: string
  description: string
  teacher: Teacher
  courseno: int
  status: ProjectStatus
  prerequisites: string seq
  cosupervisors: Teacher seq
}


[<CLIMutable>]
type StudentReference = {
  studynumber: string
}

[<CLIMutable>]
type Group = {
  id: ObjectId
  number: int
  students: Student seq
  projectId: ObjectId
  wishList: ObjectId seq
}
