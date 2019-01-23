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

[<CLIMutable>]
type GroupInput = {
  number: int
}

[<CLIMutable>]
type StudentGroupInput = {
  groupNumber: int
  studentId: string
}