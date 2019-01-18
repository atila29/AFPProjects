namespace GuardedCommands.Backend
// Michael R. Hansen 05-01-2016, 04-01-2018
// This file is obtained by an adaption of the file MicroC/Comp.fs by Peter Sestoft
open System
open Machine

open GuardedCommands.Frontend.AST
module CodeGeneration =


(* A global variable has an absolute address, a local one has an offset: *)
   type Var = 
     | GloVar of int                   (* absolute address in stack           *)
     | LocVar of int                   (* address relative to bottom of frame *)

(* The variable environment keeps track of global and local variables, and 
   keeps track of next available offset for local variables *)

   type varEnv = Map<string, Var*Typ> * int

(* The function environment maps function name to label and parameter decs *)

   type ParamDecs = (Typ * string) list
   type funEnv = Map<string, label * Typ option * ParamDecs>

/// CE vEnv fEnv e gives the code for an expression e on the basis of a variable and a function environment
   let rec CE vEnv fEnv = 
       function
       | N n          -> [CSTI n]
       | B b          -> [CSTI (if b then 1 else 0)]
       | Access acc   -> CA vEnv fEnv acc @ [LDI] 

       | Apply("-", [e]) -> CE vEnv fEnv e @  [CSTI 0; SWAP; SUB]
         
       | Apply("!", [b]) -> CE vEnv fEnv b @ [NOT]

       | Apply("&&",[b1;b2]) -> let labend   = newLabel()
                                let labfalse = newLabel()
                                CE vEnv fEnv b1 @ [IFZERO labfalse] @ CE vEnv fEnv b2
                                @ [GOTO labend; Label labfalse; CSTI 0; Label labend]

       | Apply("||", [b1;b2]) -> let labend  = newLabel()
                                 let labtrue = newLabel()
                                 CE vEnv fEnv b1 @ [IFNZRO labtrue] @ CE vEnv fEnv b2
                                 @ [GOTO labend; Label labtrue; CSTI 1; Label labend]


       | Apply(o,[e1;e2]) when List.exists (fun x -> o=x) ["-"; "+"; "*"; "%"; "/"; "="; "<"; ">"; "<="; ">="; "<>"]
                             -> let ins = match o with
                                          | "-" ->  [SUB]
                                          | "+"  -> [ADD]
                                          | "*"  -> [MUL]
                                          | "/"  -> [DIV]
                                          | "%"  -> [MOD]
                                          | "="  -> [EQ]
                                          | "<>" -> [EQ; NOT]
                                          | "<"  -> [LT]
                                          | ">"  -> [SWAP; LT]
                                          | "<=" -> [SWAP; LT; NOT]
                                          | ">=" -> [LT; NOT] 
                                          | _    -> failwith "CE: this case is not possible"
                                CE vEnv fEnv e1 @ CE vEnv fEnv e2 @ ins 

       | Apply(fname, es)    -> let label = match Map.tryFind fname fEnv with
                                                                | Some(flabel,_,_) -> flabel
                                                                | _ -> failwith (String.concat " " [ "function"; fname; "not defined"])
                                (es |> List.collect (CE vEnv fEnv)) @ [CALL (es.Length, label)]

       | Addr(acc)           -> CA vEnv fEnv acc
       | _            -> failwith "CE: not supported yet"
       

/// CA vEnv fEnv acc gives the code for an access acc on the basis of a variable and a function environment
   and CA vEnv fEnv = function | AVar x         -> match Map.find x (fst vEnv) with
                                                   | (GloVar addr,_) -> [CSTI (addr + 2)]
                                                   | (LocVar addr,_) -> [GETBP; CSTI addr; ADD]
                               | AIndex(acc, e) -> CA vEnv fEnv acc @ [LDI] @ CE vEnv fEnv e @ [ADD]
                               | ADeref e       -> CE vEnv fEnv e

  
(* Bind declared variable in env and generate code to allocate it: *)   
   let allocate (kind : int -> Var) (typ, x) (vEnv : varEnv)  =
    let (env, fdepth) = vEnv
    match typ with
    | ATyp (ATyp _, _) -> 
      raise (Failure "allocate: array of arrays not permitted")
    | ATyp (t, Some i) -> 
        let newEnv = Map.add x (kind (fdepth+i), typ) env, fdepth+i+1
        let code = [INCSP i; GETSP; CSTI (i-1); SUB]
        (newEnv, code)
    | _ -> 
      let newEnv = (Map.add x (kind fdepth, typ) env, fdepth+1)
      let code = [INCSP 1]
      (newEnv, code)
                      
/// CS vEnv fEnv s gives the code for a statement s on the basis of a variable and a function environment                          
   let rec CS vEnv fEnv = function
       | PrintLn e      -> CE vEnv fEnv e @ [PRINTI; INCSP -1] 

       | Ass(acc,e)     -> CA vEnv fEnv acc @ CE vEnv fEnv e @ [STI; INCSP -1]

       | Block([],stms) -> CSs vEnv fEnv stms

       | Block(decs,stms) -> let rec lVarCode lvEnv code decs =
                                match decs with
                                | d::ds -> match d with 
                                           | VarDec(typ,name) -> 
                                                                 let (newEnv, newCode) = allocate LocVar (typ, name) lvEnv
                                                                 lVarCode newEnv (code @ newCode) ds
                                           | _ -> failwith "only variable declarations allowed in a block"
                                | [] -> (lvEnv, code)
                             let (lvEnv, vCode) = lVarCode vEnv [] decs
                             vCode @ CSs lvEnv fEnv stms @ [INCSP -decs.Length]

       | Alt(GC([]))    -> [CSTI -1; STOP]

       | Alt(GC(alts))  -> let endLabel = newLabel()
                           (alts |> List.map (fun (e, s) ->
                               let stmSkipLabel = newLabel()
                               CE vEnv fEnv e @ [IFZERO stmSkipLabel] @ CSs vEnv fEnv s @ [GOTO endLabel; Label stmSkipLabel]
                           ) |> List.collect id) @ [CSTI -1; STOP] @ [Label endLabel]
                                
       | Do(GC(alts))   -> let startLabel = newLabel() 
                           [Label startLabel] @ (alts |> List.map (fun (e, s) ->
                               let stmSkipLabel = newLabel()
                               CE vEnv fEnv e @ [IFZERO stmSkipLabel] @ CSs vEnv fEnv s @ [GOTO startLabel; Label stmSkipLabel]
                           ) |> List.collect id)
       
       | Return(Some(exp)) ->   let (_, m) = vEnv
                                CE vEnv fEnv exp @ [RET m]
        
       | Call(pname,es)    -> let label = match Map.tryFind pname fEnv with
                                           | Some(flabel,_,_) -> flabel
                                           | _ -> failwith (String.concat " " [ "procedure"; pname; "not defined"])
                              (es |> List.collect (CE vEnv fEnv)) @ [CALL (es.Length, label); INCSP -1]

       | _              -> failwith "CS: this statement is not supported yet"

   and CSs vEnv fEnv stms = List.collect (CS vEnv fEnv) stms 

   /// CF vEnv fEnv gives code for a global function based on a function declaration
   let CF vEnv (fEnv : funEnv) = function
        | FunDec(rtype, fname, _, stm) ->   match Map.tryFind fname fEnv with
                                            | Some(label, _, paramDecs) -> let rec paramCode pDecs lVEnv =
                                                                                match pDecs with
                                                                                | v::vs -> let (newEnv, _) = allocate LocVar v lVEnv
                                                                                           paramCode vs newEnv
                                                                                | []    -> lVEnv
                                                                           let lEnv = paramCode paramDecs (fst(vEnv), 0)
                                                                           let code = [Label label] @ CS lEnv fEnv stm
                                                                           match rtype with
                                                                           | None ->  code @ [RET (paramDecs.Length - 1)]
                                                                           | _ -> code
                                            | _ -> failwith "function not declared"                                   
        | _ -> failwith "not a function"
   
   /// CFs vEnv fEnv gives code for all function declarations contained in given list
   let CFs vEnv (fEnv : funEnv) decs =
        let rec addF = function
            | dec::decs -> 
                match dec with
                | FunDec(_,_,_,_) -> (CF vEnv fEnv dec) @ addF decs
                | _ -> addF decs
            | _ -> []
        addF decs


(* ------------------------------------------------------------------- *)

(* Build environments for global variables and functions *)

   let makeGlobalEnvs decs = 
       let rec addDec decs vEnv fEnv = 
           match decs with 
           | []         -> (vEnv, fEnv, [])
           | dec::decr  -> 
             match dec with
             | VarDec (typ, var) -> let (vEnv1, initCode1) = allocate GloVar (typ, var) vEnv
                                    let (vEnv2, fEnv2, initCode2) = addDec decr vEnv1 fEnv
                                    (vEnv2, fEnv2, initCode1 @ initCode2)
             | FunDec (tyOpt, f, paramDecs, body) -> let vardecs = (paramDecs |> List.map (fun p -> 
                                                                                        match p with
                                                                                        | VarDec(t, n) -> (t, n)
                                                                                        | _ -> failwith "Only variable declarations supported in functions"))
                                                     addDec decr vEnv (Map.add f (newLabel(), tyOpt, vardecs) fEnv)
       let (vEnv, fEnv, initCode) = addDec decs (Map.empty, 0) Map.empty
       let postCode = CFs vEnv fEnv decs
       (vEnv, fEnv, initCode, postCode)
         

/// CP prog gives the code for a program prog
   let CP (P(decs,stms)) = 
        let _ = resetLabels ()
        let ((gvM,_) as gvEnv, fEnv, initCode, postCode) = makeGlobalEnvs decs
        let mainLabel = newLabel()
        [CALL (0, mainLabel); Label mainLabel] @ initCode @ CSs gvEnv fEnv stms @ [STOP] @ postCode



