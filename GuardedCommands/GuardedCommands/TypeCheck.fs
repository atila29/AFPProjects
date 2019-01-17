namespace GuardedCommands.Frontend
// Michael R. Hansen 06-01-2016 , 04-01-2018

open System
open Machine
open GuardedCommands.Frontend.AST

module TypeCheck = 

/// tcE gtenv ltenv e gives the type for expression e on the basis of type environments gtenv and ltenv
/// for global and local variables 
   let rec tcE gtenv ltenv = function                            
         | N _              -> ITyp   
         | B _              -> BTyp   
         | Access acc       -> tcA gtenv ltenv acc     
         | Addr acc         -> tcA gtenv ltenv acc
         | Apply(f, [ec; et; ef]) when f = "TENARY" -> match (tcE gtenv ltenv ec, tcE gtenv ltenv et, tcE gtenv ltenv ef) with
                                                         | (BTyp, tt, tf) when tt=tf -> tt
                                                         | _ -> failwith "condition should be boolean, true and false should have matching types"
         | Apply(f,[e]) when List.exists (fun x -> x=f) ["-"; "!"]  
                            -> tcMonadic gtenv ltenv f e        

         | Apply(f,[e1;e2]) when List.exists (fun x -> x=f) ["-";"+";"*"; "/"; "%"; "<"; ">"; "<>"; "="; "&&"; "||";]    
                            -> tcDyadic gtenv ltenv f e1 e2   
         
         | Apply(f, es)  -> tcNaryFunction gtenv ltenv f es

         | _                -> failwith "tcE: not supported yet"

   and tcMonadic gtenv ltenv f e = match (f, tcE gtenv ltenv e) with
                                   | ("-", ITyp) -> ITyp
                                   | ("!", BTyp) -> BTyp
                                   | _           -> failwith "illegal/illtyped monadic expression" 
   
   and tcDyadic gtenv ltenv f e1 e2 = match (f, tcE gtenv ltenv e1, tcE gtenv ltenv e2) with
                                      | (o, ITyp, ITyp) when List.exists (fun x ->  x=o) ["-";"+";"*";"/";"%"]  -> ITyp
                                      | (o, ITyp, ITyp) when List.exists (fun x ->  x=o) ["<>";"<";">";"="]  -> BTyp
                                      | (o, BTyp, BTyp) when List.exists (fun x ->  x=o) ["<>";"&&";"||";"="]  -> BTyp 
                                      | _                      -> failwith("illegal/illtyped dyadic expression: " + f)

   and tcNaryFunction gtenv ltenv f es =
        let (argtypes, rtype) = match Map.tryFind f gtenv with
                                | Some(FTyp(types, Some(rtype))) -> (types, rtype)
                                | _ -> failwith ("function " + f + " not defined or is procedure")
        if not (List.forall2 (fun a e -> match a, tcE gtenv ltenv e with
                                            | (ATyp (aTyp, _), ATyp (eTyp, _)) -> aTyp = eTyp
                                            | _ -> a = tcE gtenv ltenv e) 
                                            argtypes es)
        then failwith "argument and parameter types does not match"
        rtype

   and tcNaryProcedure gtenv ltenv f es = failwith "type check: procedures not supported yet"
      

/// tcA gtenv ltenv e gives the type for access acc on the basis of type environments gtenv and ltenv
/// for global and local variables 
   and tcA gtenv ltenv = 
         function 
         | AVar x         -> match Map.tryFind x ltenv with
                              | None   -> match Map.tryFind x gtenv with
                                          | None   -> failwith ("no declaration for : " + x)
                                          | Some t -> t
                              | Some t -> t 
         | AIndex(acc, e) -> match tcE gtenv ltenv e with
                              | ITyp -> match (tcA gtenv ltenv acc) with
                                          | ATyp (t,_)-> t
                                          | _ -> failwith "Should never happen?"
                              | _ -> failwith "Index needs to be of type integer"
         | ADeref e       -> failwith "tcA: pointer dereferencing not supported yes"
 

/// tcS gtenv ltenv retOpt s checks the well-typeness of a statement s on the basis of type environments gtenv and ltenv
/// for global and local variables and the possible type of return expressions 
   and tcS gtenv ltenv = function                           
                         | PrintLn e -> ignore(tcE gtenv ltenv e)
                         | Ass(acc,e) -> if tcA gtenv ltenv acc = tcE gtenv ltenv e
                                         then ()
                                         else failwith "illtyped assignment"                                
                         | Block([],stms) -> List.iter (tcS gtenv ltenv) stms
                         | Block(decs,stms) -> List.iter (tcS gtenv (tcLDecs gtenv ltenv decs)) stms
                         // Task 3.6
                         | Alt(GC(alts)) | Do(GC(alts)) ->  let es, stms = List.unzip alts
                                                            if not (List.forall (fun e -> tcE gtenv ltenv e = BTyp) es)
                                                            then failwith "guard is not a boolean expression"
                                                            stms |> List.iter (fun sl -> List.iter (tcS gtenv ltenv) sl)
                         | Return None                  -> ()
                         | Return (Some e)              -> ignore(tcE gtenv ltenv e)
                         | _        -> failwith "tcS: this statement is not supported yet"


//// checks well-typeness of global declarations, and returns new global declarations
   and tcGDec gtenv = function  
                      | VarDec(t,s)             -> Map.add s t gtenv
                      | FunDec(None,f,decs,stm) -> failwith "procedures not supported"
                      // A function is well-typed if:
                      // - the formal parameters are different (Done)
                      // - every return statement has declared return type
                      // - statement stm is well-typed (Done)
                      | FunDec(Some(t),f,decs,stm) -> let rec paramTypes = function 
                                                        | VarDec(dt, dn)::ds -> dt::(paramTypes ds)
                                                        | d::ds -> paramTypes ds
                                                        | [] -> []
                                                      let paramTypes = paramTypes decs
                                                      let ltenv = tcLDecs gtenv Map.empty decs 
                                                      if ltenv.Count <> decs.Length  // Slettes? Ikke relevant for typecheck
                                                      then failwith ("identical parameters defined in function " + f)
                                                      let ftyp = FTyp(paramTypes, Some(t))
                                                      let gtenv = Map.add f ftyp gtenv // Add to gtenv to allow for recursive functions
                                                      tcS gtenv ltenv stm
                                                      gtenv

                                                           
//// checks well-typeness of a global declaration list, and returns new global type environment
   and tcGDecs gtenv = function
                       | dec::decs -> tcGDecs (tcGDec gtenv dec) decs
                       | _         -> gtenv

/// checks well-typeness of a local declaration, and returns new local type environment
   and tcLDec gtenv ltenv = function
                            | VarDec(t,s) -> Map.add s t ltenv
                            | _ -> failwith "only local variable declarations supported"

/// checks well-typeness of a local declaration list, and returns new local type environment
   and tcLDecs gtenv ltenv = function
                             | dec::decs -> tcLDecs gtenv (tcLDec gtenv ltenv dec) decs
                             | _ -> ltenv

/// tcP prog checks the well-typeness of a program prog
   and tcP(P(decs, stms)) = let gtenv = tcGDecs Map.empty decs
                            List.iter (tcS gtenv Map.empty) stms

  
