module Platform.Model



[<CLIMutable>]
type Request = {
  id: int option
  title: string
  description: string
  teacher: string
}
