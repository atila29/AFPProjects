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
    match exp with
    | N n           -> Node(string n, [])
    | B b           -> Node(string b, [])
    | Access a      -> transformAccess a
    | Addr a        -> Node("reference", [transformAccess a])
    | Apply (s, es) -> Node("fun", es |> List.map transformExp) // Should we use s?
and transformAccess (access: Access) =
        match access with
        | AVar x        -> Node("Var", [])
        | AIndex (a, e) -> Node("Index", [transformAccess a])
        | ADeref e      -> Node("Pointer", [transformExp e])

let rec transformStm (stm: Stm) =
    let transformGc (gc: GuardedCommand) =
        match gc with GC cmds -> Node ("GCs", cmds |> List.map (fun (e, stms) -> Node("GS", [Node("Condition", [transformExp e]); Node("Statements", stms |> List.map transformStm)])))
    
    match stm with
    | PrintLn e -> Node("PrintLn", [transformExp e])
    | Ass (a, e) -> Node("Assignment", [transformAccess a ; transformExp e ])
    | Return e when e.IsSome -> Node("Return", [transformExp e.Value])
    | Return _ -> Node("Void", [])
    | Alt gc -> transformGc gc
    | Do gc -> transformGc gc
    | Block (ds, ss) -> Node("Block", [
        Node("Declarations", ds |> List.map transformDec);
        Node("Statements", ss |> List.map transformStm);
    ])
    | Call (s, es)-> Node(s, es |> List.map transformExp)
and transformDec (dec: Dec) =
    let rec transformType (typ: Typ) =
        let rec transformTypeAcc (tl: Typ list) acc =
            match tl with 
            | []-> acc
            | x::xs -> transformTypeAcc xs (Node("Procedure", [transformType x]))
        match typ with
        | ITyp -> Node("Int", [])
        | BTyp -> Node("Bool", [])
        | ATyp (t, oi) when oi.IsSome -> Node("Array", [transformType t; Node(string oi.Value, [])])
        | ATyp (t, _) -> Node("Array", [transformType t])
        | PTyp t -> Node("TypePoint", [transformType t])
        | FTyp (ts, t) when t.IsSome ->transformTypeAcc ts (Node("FType",  [Node("Option", [transformType t.Value])]))
        | FTyp (ts, _) -> transformTypeAcc ts (Node("FType", []))
    match dec with
    | VarDec (t, s) -> Node("VarDec", [Node(s,[]); transformType t])
    | FunDec (t, s, ds, stm) when t.IsSome-> Node("FunDec", [
        transformType t.Value; 
        Node("Name", [Node(s, [])]); 
        Node("Declarations", ds |> List.map transformDec);
        transformStm stm
        ])
    | FunDec (_, s, ds, stm) -> Node("FunDec", [
        Node("Name", [Node(s, [])]); 
        Node("Declarations", ds |> List.map transformDec);
        transformStm stm
        ])

let transformProgram (program: Program) =
    match program with
    | P(ds, stmts) -> Node("Program", [Node("Declarations", ds |> List.map transformDec); Node("", stmts |> List.map transformStm)])

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

