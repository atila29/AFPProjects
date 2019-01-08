module Tree

open System
open AST

type Tree<'a> = Node of 'a * ('a Tree list)

let movetree (Node((label, x), subtrees), x':float) =
    Node((label, x+x'), subtrees)

type Extent = (float * float) list

let moveextent (e:Extent, x) = e |> List.map (fun (p, q) -> (p + x, q + x))

let rec merge i =
    match i with
    | ([], qs) -> qs
    | (ps, []) -> ps
    | ((p,_)::ps, (_,q)::qs) -> (p,q)::merge((ps, qs))


let mergelist (es: Extent list) = List.foldBack (fun x acc -> merge (x, acc)) es []

let rec fit a b =
    match (a, b) with
    | ((_,p)::ps, (q,_)::qs) -> max (fit ps qs) (p - q + 1.0)
    | _ -> 0.0

let fitlistl es =
    let rec fitlistl' acc i =
        match i with
        | [] -> []
        | e::es ->
            let x = fit acc e 
                in x::fitlistl' (merge (acc, moveextent (e,x))) es
    in fitlistl' [] es

let fitlistr es =
    let rec fitlistr' acc i =
        match i with
        | [] -> []
        | e::es ->
            let x = - fit acc e 
                in x::fitlistr' (merge (moveextent (e,x), acc)) es
    in List.rev (fitlistr' [] es)

let fitlist es = 
    List.zip (fitlistl es) (fitlistr es) 
    |> List.map(fun (x, y) -> (x+y)/2.0)

let design tree =
    let rec design' (Node(label, subtrees)) =
        let (trees, extents) = List.unzip (List.map design' subtrees)
        let positions = fitlist extents
        let ptrees = List.zip trees positions |> List.map(fun (x, y) -> movetree (x,y))
        let pextents = List.zip extents positions |> List.map(fun (x, y) -> moveextent (x,y))
        let resultextent = (0.0, 0.0)::(mergelist pextents)
        let resulttree = Node((label, 0.0), ptrees)
        in (resulttree, resultextent)
    in fst (design' tree)

let rec transformExp (exp: Exp) =
    let rec transformAccess (access: Access) =
        match access with
        | AVar x        -> Node("Var", [])
        | AIndex (a, e) -> Node("Index", [transformAccess a])
        | ADeref e      -> Node("Pointer", [transformExp e])
    match exp with
    | N n -> Node("Int", [])
    | B _ -> Node("Bool", [])
    | Access a -> transformAccess a
    | Addr a -> Node("reference", [transformAccess a])
    | Apply (s, es) -> Node("fun", es |> List.map transformExp)

[<EntryPoint>]
let main argv =
    let node5 = Node("5", [])
    let node4 = Node("4", [node5])
    let node3 = Node("3", [node4])
    let node2 = Node("2", [])
    let node1 = Node("1", [])
    let root = Node("root", [node1;node2;node3])
      
    let result = design root
    //printf "%s" (design node1)

    0 // return an integer exit code

