module Tree

open System.Text
open System.IO
open AST


type Tree<'a> = Node of 'a * ('a Tree list)

let movetree (Node((label, x), subtrees), x':float) =
    Node((label, x+x'), subtrees)

type Extent = (float * float) list

let moveextent (e:Extent, x) = e |> List.map (fun (p, q) -> (p + x, q + x))

let rec merge i =
    match i with
    | [], qs -> qs
    | ps, [] -> ps
    | (p,_)::ps, (_,q)::qs -> (p,q)::(merge(ps, qs))

let mergelist (es: Extent list) = List.fold (fun acc x -> merge (acc, x)) [] es

let rec fit a b =
    match (a, b) with
    | ((_,p)::ps, (q,_)::qs) -> max (fit ps qs) (p - q + 1.0)
    | _ -> 0.0

let fitlistl es =
    let rec fitlistl' acc i =
        match acc, i with
        | acc, [] -> []
        | acc, e::es ->
            let x = fit acc e 
                in x::(fitlistl' (merge (acc, moveextent (e,x))) es)
    in fitlistl' [] es

let fitlistr es =
    let rec fitlistr' acc i =
        match acc, i with
        | acc, [] -> []
        | acc, e::es ->
            let x = -(fit e acc) 
                in x::(fitlistr' (merge (moveextent (e,x), acc)) es)
    in List.rev (fitlistr' [] (List.rev es))

let fitlist es = 
    List.map (fun (x, y) -> (x+y)/2.0) (List.zip (fitlistl es) (fitlistr es))

let rec design' (Node(label, subtrees)) =
    let (trees, extents) = List.unzip (List.map design' subtrees)
    let positions = fitlist extents
    let ptrees = List.map movetree (List.zip trees positions)
    let pextents = List.map moveextent (List.zip extents positions)
    let resultextent = (0.0, 0.0)::(mergelist pextents)
    let resulttree = Node((label, 0.0), ptrees)
    in (resulttree, resultextent)

let design tree = fst(design' tree)


let appendLine (a:StringBuilder) (b) = a.Append (string b + "\n")

let rec appendLines (a:StringBuilder) (b) =
    match b with
    | x::xs -> appendLines (a.Append (x + "\n")) xs
    | _ -> a

let drawTreePS (resultTree:Tree<string * float>) = 
    let verticalSep = 40
    let horizontalSep = 100
    let lblHeight = 18
    let header = ["%!";"1 1 scale";"700 999 translate";"newpath";"/Times-Roman findfont 10 scalefont setfont"]
    let mutable fileString = StringBuilder()
    fileString <- appendLines fileString header

    let getOffsets nodes = nodes |> List.map (fun (Node((_, offset), _)) -> offset)

    let getOffsetRange offsets =
        match offsets with
        | x::xs -> (List.min(offsets), List.max(offsets))
        | _ -> (0.0, 0.0)
    
    let drawLabel (lbl, x, y) =
        fileString <- appendLine fileString (sprintf "%d %d moveto" (int x) (int y))
        fileString <- appendLine fileString (sprintf " (%s) dup stringwidth pop 2 div neg 0 rmoveto show" lbl)
   
    let rec drawLines (node:Tree<string * float>, x:int, y:int) = 
        match node with
        |Node ((lbl, _), []) ->
            // draw vertical line above label
            fileString <- appendLine fileString (sprintf "%d %d moveto" x y)
            fileString <- appendLine fileString (sprintf " %d %d lineto" x (y - verticalSep / 2))
            // draw label
            drawLabel (lbl, x, y - verticalSep / 2 - lblHeight / 2)
            fileString <- appendLine fileString "stroke"
        | Node((lbl, _), children) ->
            // draw vertical line above label
            fileString <- appendLine fileString (sprintf "%d %d moveto" x y)
            fileString <- appendLine fileString (sprintf " %d %d lineto" x (y - verticalSep / 2))
            // draw label
            drawLabel (lbl, x, y - verticalSep / 2 - lblHeight / 2)
            // draw vertical line below label
            fileString <- appendLine fileString (sprintf "%d %d moveto" x (y - verticalSep / 2 - int(float lblHeight * 0.7)))
            fileString <- appendLine fileString (sprintf " %d %d lineto" x (y - verticalSep))
            // calculate translated offsets
            let low, high = getOffsetRange (getOffsets children)
            let tLow = x + int (low * (float horizontalSep))
            let tHigh = x + int (high * (float horizontalSep))
            // draw horizontal line between children
            if children.Length > 0 then
                fileString <- appendLine fileString (sprintf "%d %d moveto" tLow (y - verticalSep))
                fileString <- appendLine fileString (sprintf " %d %d lineto" tHigh (y - verticalSep))
            fileString <- appendLine fileString "stroke"
            // draw children
            children |> List.map (fun c ->
                match c with
                | Node((_, childOffset), _) ->
                    let childX = x + int (childOffset * (float horizontalSep))
                    drawLines (c, childX, (y - verticalSep))) |> ignore
          
    drawLines (resultTree, 0, 0)
    fileString <- appendLine fileString "showpage"
    fileString.ToString()


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
        match typ with
        | ITyp -> Node("Int", [])
        | BTyp -> Node("Bool", [])
        | ATyp (t, oi) when oi.IsSome -> Node("Array", [transformType t; Node(string oi.Value, [])])
        | ATyp (t, _) -> Node("Array", [transformType t])
        | PTyp t -> Node("TypePoint", [transformType t])
        | FTyp (ts, t) when t.IsSome -> Node("FType",  [Node("Option", [transformType t.Value]); Node("Procedure", ts |> List.map transformType)])
        | FTyp (ts, _) -> Node("FType",  [Node("Procedure", ts |> List.map transformType)])
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
    | P(ds, stmts) -> Node("Program", [Node("Declarations", ds |> List.map transformDec); Node("Statements", stmts |> List.map transformStm)])

// let rand = new System.Random()
// let generateAST depth width =
//     let generateExp w =
//         match w with
//         | 1 -> N(rand.Next())
//         | n when w -> 


//     let generateStmt w = 
//         match w with
//         | 1 -> PrintLn
//     let root = P 


let rec generateAST width depth =
    let rec generateRec rW rD =
        match rD, rW with
        | 0, w -> [], []
        | 1, w -> [], List.init w (fun i -> Block([], [])) 
        | 2, w -> (List.init w (fun i -> VarDec(ITyp, string i)), [])
        | d, w -> [VarDec(ITyp, "n")], List.init (w - 1) (fun index -> Block(generateAST w (d - 3)))
    (generateRec width depth)


[<EntryPoint>]
let main argv =
    // let node8 = Node("8", [])
    // let node9 = Node("9", [])
    // let node6 = Node("6", [])
    // let node7 = Node("7", [node8;node9])
    // let node5 = Node("5", [])
    // let node4 = Node("4", [node5])
    // let node3 = Node("3", [node4])
    // let node2 = Node("2", [])
    // let node1 = Node("1", [node7])
    // let root = Node("root", [node1;node2;node3])

    let p0 = P([
        VarDec(ITyp, "i0");
        FunDec(Some(PTyp(BTyp)), "f0", [], Alt(GC([
            (
            Apply("+", [N(2); N(3)]),
            [
                Return(None)
            ]
            )
        ])))
    ], [
        PrintLn(
            N(32)
        );
        Block([
            VarDec(BTyp, "b1");
            VarDec(ATyp(ITyp, Some(32)), "array")
        ], [
            Ass(AVar("ads"), N(2));
            Return(Some(Addr(AVar("t23"))))
        ])
    ])

    let p1 = P([
        VarDec(ITyp, "a");
        VarDec(ITyp, "b")
        VarDec(ITyp, "result")
    ], [
        Ass(AVar("a"), N(4));
        Ass(AVar("b"), N(7));
        Ass(AVar("result"), Apply("+", [Access(AVar("a")); Access(AVar("b"))]))
        PrintLn(Access(AVar("result")))
    ])

    let problemRoot = Node("root", [
            Node("left", [Node("a", []); Node("b", [])]);
            Node("center", [Node("c", []); Node("right", [])]);
            Node("right", []);
    ])
    let problemTree = design problemRoot
    File.WriteAllText("problem.ps", drawTreePS problemTree)

    let tree =  (design (transformProgram p1))
    let drawing = drawTreePS tree

    File.WriteAllText("p0.ps", drawTreePS (design (transformProgram p0)))
    File.WriteAllText("p1.ps", drawTreePS (design (transformProgram p1)))

      
    let p = drawTreePS (design (transformProgram (P(generateAST 5 5))))
    File.WriteAllText("ast1.ps", p)

    // let result = design root
    // let contents = drawTreePS result
    // File.WriteAllText("test.ps", contents)
    0 // return an integer exit code

