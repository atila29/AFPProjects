module Platform.Model.Domain

type User =
    | Student of Student
    | Teacher of Teacher
    | HeadOfStudy of HeadOfStudy

and Student = string
and Teacher = string
and HeadOfStudy = string

type Group = int * Student list

type Prerequisites = string list
type Restriction =
| MaxGroups of int
| MaxGroupSize of int
| AbitraryRule of string * int option

type Title = string
type Desc = string

type Project = int * Title * Desc * Teacher * Prerequisites option * Restriction list * Teacher list

type Request = Group * (Project * int) list

(*
List:
    SubmitRequest (must)
    InspectProject (must)
    SubmitProjectProposal (must)
    AcceptProjectProposal (must)
    RejectProjectProposal (must)
    PublishProposal(s) (must)
    AssignProjectToGroup (must)
    
    Matchmaking (could)

    ReviseProject (teacher) (could)

    Communicate (could)

    Login (Could
*)

