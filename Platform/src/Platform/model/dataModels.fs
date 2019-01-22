module Platform.Model.Data

[<CLIMutable>]
type Project = {
  id: int option
  title: string
  description: string
  teacher: string
}
