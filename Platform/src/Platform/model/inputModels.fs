module Platform.Model.Input

open Platform.Model.Data
open MongoDB.Bson

// Input models rely on some data models

[<CLIMutable>]
type ProjectProposal = {
  title: string
  description: string
  teacherEmail: string
  prerequisitesCS: string
  cosupervisorsEmailCS: string
}

[<CLIMutable>]
type StudentReference = {
  studynumber: string
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
  studynumber: string
}

[<CLIMutable>]
type ProjectGroupInput = {
  groupNumber: int
  projectId: string
}

[<CLIMutable>]
type StudentInput = {
    name: string
    studynumber: string
}
