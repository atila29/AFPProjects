module Platform.Model.Data

[<CLIMutable>]
type RequestData = {
  id: int option
  title: string
  description: string
  teacher: string
}
