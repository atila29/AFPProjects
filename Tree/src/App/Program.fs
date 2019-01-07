open System

type Tree<'a> = Node of 'a * ('a Tree list)

let movetree (Node((label, x), subtrees), x':float) =
    Node((label, x+x'), subtrees)

type Extent = (float * float) list

let moveextent (e:Extent, x) = e |> List.map (fun (p, q) -> (p + x, q + x))

let rec merge i =
    match i with
    | ([], qs) -> qs
    | (ps, []) -> ps
    | ((p,_)::ps, (_,q)::qs) -> (p,q)::merge(ps, qs)


[<EntryPoint>]
let main argv =
    Library.Say.hello "hej"
    0 // return an integer exit code

