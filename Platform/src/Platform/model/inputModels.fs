module Platform.Model.Input



[<CLIMutable>]
type ProjectProposal = {
  id: int option
  title: string
  description: string
  teacher: string
}
