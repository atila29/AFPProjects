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

[<CLIMutable>]
type AnswerProposalInput = {
  id: string
  courseNo: int option
}
