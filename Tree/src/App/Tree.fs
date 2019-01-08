module Tree

open System.Text
open System.IO

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



let appendLine (a:StringBuilder) (b) = a.Append (string b + "\n")

let rec appendLines (a:StringBuilder) (b) =
    match b with
    | x::xs -> appendLines (a.Append (x + "\n")) xs
    | _ -> a


let drawTreePS (resultTree:Tree<string * float>) = 
    let verticalSep = 40
    let horizontalSep = 20
    let lblHeight = 15
    let header = ["%!";"1 1 scale";"700 999 translate";"newpath";"/Times-Roman findfont 10 scalefont setfont"]
    let mutable fileString = StringBuilder()
    fileString <- appendLines fileString header
    
    let getOffsets nodes = nodes |> List.map (fun (Node((_, offset), _)) -> offset)

    let getOffsetRange offsets =
        match offsets with
        | x::xs -> (List.min(offsets), List.max(offsets))
        | _ -> (0.0, 0.0)

    let translateOffsets (offsets:float list, x) = 
        let low, high = getOffsetRange offsets
        let diff = abs(high - low)
        let length = diff * float(horizontalSep * offsets.Length)
        let tHigh = float(x) + length / abs(high) / diff 
        let tLow = float(x) - length + tHigh
        (int(tLow), int(tHigh))
    
    let drawLabel (lbl, x, y) =
        fileString <- appendLine fileString (sprintf "%d %d moveto" (int x) (int y))
        fileString <- appendLine fileString (sprintf " (%s) dup stringwidth pop 2 div neg 0 rmoveto show" lbl)
   
    let rec drawLines (node:Tree<string * float>, x:int, y:int) = 
        match node with
        | Node((lbl, _), children) ->
            let vertSegmentLength = ((verticalSep - lblHeight) / 2)
            // draw vertical line above label
            fileString <- appendLine fileString (sprintf "%d %d moveto" x y)
            fileString <- appendLine fileString (sprintf " %d %d lineto" x (y - vertSegmentLength))
            // draw label
            drawLabel (lbl, x, y - vertSegmentLength - lblHeight/2)
            // draw vertical line below label
            fileString <- appendLine fileString (sprintf "%d %d moveto" x (y - vertSegmentLength - lblHeight))
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
                    let childX = int(float(translatedOffsetDiff) * childOffset / offsetDiff)
                    drawLines (c, childX, (y - verticalSep))) |> ignore
            fileString

    drawLines (resultTree, 0, 0) |> ignore
    fileString <- appendLine fileString "showpage"
    fileString.ToString()

[<EntryPoint>]
let main argv =
    let node8 = Node("8", [])
    let node7 = Node("7", [])
    let node6 = Node("6", [])
    let node5 = Node("5", [])
    let node4 = Node("While", [node5])
    let node3 = Node("Seq", [node4])
    let node2 = Node("VarDec", [node6;node7;node8])
    let node1 = Node("Seq", [])
    let root = Node("Block", [node1;node2;node3])
      
    let result = design root
    let contents = drawTreePS result
    File.WriteAllText("test.ps", contents)
    0