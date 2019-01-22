module Platform.Model.Data

open MongoDB.Bson

[<CLIMutable>]
type ProjectData = {
  id: BsonObjectId
  title: string
  description: string
  teacher: string
  courseno: int option
}

[<CLIMutable>]
type StudentData = {
  id: string
}