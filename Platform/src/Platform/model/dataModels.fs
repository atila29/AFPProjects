module Platform.Model.Data

open MongoDB.Bson
open MongoDB.Bson
open Newtonsoft.Json.Bson

type ProjectStatus = 
  | Request=1
  | Accepted=2
  | Declined=3

[<CLIMutable>]
type ProjectData = {
  id: ObjectId
  title: string
  description: string
  teacher: string
  courseno: int
  status: ProjectStatus
}

[<CLIMutable>]
type StudentData = {
  id: string
}

[<CLIMutable>]
type GroupData = {
  id: ObjectId
  number: int
  students: StudentData seq
}