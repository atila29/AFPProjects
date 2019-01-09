module Tree

open System.Text
open System.IO
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
    in design' tree


let appendLine (a:StringBuilder) (b) = a.Append (string b + "\n")

let rec appendLines (a:StringBuilder) (b) =
    match b with
    | x::xs -> appendLines (a.Append (x + "\n")) xs
    | _ -> a

let drawTreePS (resultTree:Tree<string * float>, extent:((float*float) list)) = 
    let verticalSep = 40
    let horizontalSep = 20
    let lblHeight = 18
    let header = ["%!";"1 1 scale";"700 999 translate";"newpath";"/Times-Roman findfont 10 scalefont setfont"]
    let mutable fileString = StringBuilder()
    fileString <- appendLines fileString header
    
    let rec treeWidth current level =
        match resultTree with
        | Node((_,_), []) -> 1
        | Node((_,_), children) -> children |> List.map (fun c -> treeWidth (current + 1) level) |> List.sum

    let rec maxTreeWidth tree level =
        match tree with
        | Node((_,_), []) -> 1
        | Node((_,_), children) -> children.Length + (children |> List.map (fun c -> maxTreeWidth c (level + 1)) |> List.sum)

    let getOffsets nodes = nodes |> List.map (fun (Node((_, offset), _)) -> offset)

    let getOffsetRange offsets =
        match offsets with
        | x::xs -> (List.min(offsets), List.max(offsets))
        | _ -> (0.0, 0.0)

    let translateOffsets (offsets:float list, x) =
        match offsets with
        | [] -> (x, x)
        | o::[] -> (x, x)
        | o::os -> 
            let low, high = getOffsetRange offsets
            let diff = abs(high - low)
            let length = diff * float(horizontalSep * offsets.Length)
            let tHigh = float(x) + length * abs(high) / diff 
            let tLow = float(x) - length + abs(tHigh)
            (int(tLow), int(tHigh))
    
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
            // draw horizontal line with parent in center
            let translatedOffsets = translateOffsets (getOffsets children, x)
            if children.Length > 0 then
                fileString <- appendLine fileString (sprintf "%d %d moveto" (fst translatedOffsets) (y - verticalSep))
                fileString <- appendLine fileString (sprintf " %d %d lineto" (snd translatedOffsets) (y - verticalSep))
            fileString <- appendLine fileString "stroke"
            // draw children
            let translatedOffsetDiff = snd(translatedOffsets) - fst(translatedOffsets)
            let offsetLow, offsetHigh = getOffsetRange (getOffsets children)
            let offsetDiff = offsetHigh - offsetLow
            children |> List.map (fun c ->
                match c with
                | Node((_, childOffset), _) ->
                    let childX = x + int (childOffset * (float horizontalSep))
                    //let childX = if offsetDiff <> 0.0 then int(float(translatedOffsetDiff) * childOffset / offsetDiff) + x else x
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
    | P(ds, stmts) -> Node("Program", [Node("Declarations", ds |> List.map transformDec); Node("", stmts |> List.map transformStm)])

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
    ], [
        Ass(AVar("a"), N(4));
        Ass(AVar("b"), N(7));
        PrintLn(Apply("+", [Access(AVar("a")); Access(AVar("b"))]))
    ])

    let result0 = design (transformProgram p0)
    File.WriteAllText("p0.ps", drawTreePS result0)

      
    // let result = design root
    // let contents = drawTreePS result
    // File.WriteAllText("test.ps", contents)
    0 // return an integer exit code

