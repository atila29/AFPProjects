module Platform.Model.Input



[<CLIMutable>]
type Request = {
  id: int option
  title: string
  description: string
  teacher: string
}
