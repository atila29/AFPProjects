module Platform.Model.Data

[<CLIMutable>]
type ProjectData = {
  id: int option
  title: string
  description: string
  teacher: string
}

[<CLIMutable>]
type StudentData = {
  id: string
}