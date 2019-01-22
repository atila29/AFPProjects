module Platform.Model.Data

open MongoDB.Bson
open MongoDB.Bson

type ProjectStatus = 
  | Request=1
  | Accepted=2
  | Declined=3
  | Published=4

[<CLIMutable>]
type ProjectData = {
  id: BsonObjectId
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