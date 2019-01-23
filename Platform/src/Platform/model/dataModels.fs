module Platform.Model.Data

open MongoDB.Bson
open Newtonsoft.Json.Bson


type ProjectStatus = 
  | Request=1
  | Accepted=2
  | Declined=3
  | Published=4

[<CLIMutable>]
type Teacher = {
    name: string
    email: string
}

[<CLIMutable>]
type Student = {
    name: string
    studynumber: string
}

[<CLIMutable>]
type HeadOfStudy = {
    name: string
    department: string
    email: string
}

[<CLIMutable>]
type Restriction = {
  name: string
  n: int option
}


[<CLIMutable>]
type Project = {
  id: ObjectId
  title: string
  description: string
  teacher: Teacher
  courseno: int
  status: ProjectStatus
  restrictions: Restriction list
  prerequisites: string list
  cosupervisors: Teacher list
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
}
