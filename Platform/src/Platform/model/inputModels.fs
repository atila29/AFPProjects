module Platform.Model.Input

open Platform.Model.Data
open MongoDB.Bson

// Input models rely on some data models

[<CLIMutable>]
type ProjectProposal = {
  title: string
  description: string
  teacherEmail: string
  restrictionsCS: string
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

