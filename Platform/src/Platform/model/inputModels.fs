module Platform.Model.Input

open MongoDB.Bson


[<CLIMutable>]
type ProjectProposal = {
  title: string
  description: string
  teacher: string
}

[<CLIMutable>]
type StudentInput = {
  id: string
}