﻿// Michael R. Hansen 05-01-2016; Revised 04-01-2018


// You must revise the following 3 pathes 

#r @"bin/Debug/FsLexYacc.Runtime.dll";
#r @"bin/Debug/Machine.dll";
#r @"bin/Debug/virtualMachine.dll";

#load "AST.fs"
#load "Parser.fs"
#load "Lexer.fs"
#load "TypeCheck.fs"
#load "CodeGen.fs"
#load "CodeGenOpt.fs"
#load "Util.fs"


open GuardedCommands.Util
open GuardedCommands.Frontend.TypeCheck
open GuardedCommands.Frontend.AST
open GuardedCommands.Backend.CodeGeneration

open ParserUtil
open CompilerUtil

open Machine
open VirtualMachine



System.IO.Directory.SetCurrentDirectory __SOURCE_DIRECTORY__;;

// The Ex0.gc example:
(*

 let ex0Tree = parseFromFile "Ex0.gc";;

 let _ = tcP ex0Tree;;

 let ex0Code = CP ex0Tree;; 

 let _ = go ex0Tree;;

 let _ = goTrace ex0Tree;;


 //// Parsing of Ex1.gc

 //let ex1Tree = parseFromFile "Ex1.gc";;

 //let ex4Tree = parseFromFile "Ex4.gc"
 //let ex4Code = CP ex4Tree

 //// -- is typechecked as follows:

 //let _ = tcP ex1Tree;;

 //// obtain symbolic code:
 //let ex1Code = CP ex1Tree;; 

 //// -- is executed with trace as follows:
 //let stack = goTrace ex1Tree;;

 //// -- is executed as follows (no trace):
 //let sameStack = go ex1Tree;;

 //// "All in one" parse from file, type check, compile and run 

 //let _ = exec "Ex1.gc";;

 //let _ = exec "Ex2.gc";;

 //// Test of programs covered by the fifth task using optimized compilation (Section 8.2):
 //List.iter execOpt ["Ex1.gc"; "Ex2.gc"];;

 //// All programs relating to the basic version can be parsed:
 //let pts = List.map parseFromFile ["Ex1.gc"; "Ex2.gc";"Ex3.gc"; "Ex4.gc"; "Ex5.gc"; "Ex6.gc"; "Skip.gc"];;

 //// The parse tree for Ex3.gc
 //List.item 2 pts ;;


 //// Test of programs covered by the first task (Section 3.7):
 //List.iter exec ["Ex1.gc"; "Ex2.gc";"Ex3.gc"; "Ex4.gc"; "Ex5.gc"; "Ex6.gc"; "Skip.gc"];;

let ex6Tree = parseFromFile "Ex6.gc";;
let _ = tcP ex6Tree;;
let ex6Code = CP ex6Tree;;

let ex7Tree = parseFromFile "Ex7.gc";;
let _ = tcP ex7Tree;;
let ex7Code = CP ex7Tree
let ex7Ex = goTrace ex7Tree

let a1Tree = parseFromFile "A1.gc";;
let _ = tcP a1Tree;;
let a1Code = CP a1Tree;;
goTrace a1Tree

let a0Tree = parseFromFile "A0.gc";;
let _ = tcP a0Tree;;
let a0Code = CP a0Tree;;
go a0Tree

// Test of programs covered by the second task (Section 4.3):
//List.iter exec ["Ex7.gc"; "fact.gc"; "factRec.gc"; "factCBV.gc"];;

// Test of programs covered by the fourth task (Section 5.4):
List.iter exec ["A0.gc"; "A1.gc"; "A2.gc"; "A3.gc"];;
(*
// Test of programs covered by the fifth task (Section 6.1):
List.iter exec ["A4.gc"; "Swap.gc"; "QuickSortV1.gc"];;

// Test of programs covered by the fifth task (Section 7.4):
List.iter exec ["par1.gc"; "factImpPTyp.gc"; "QuickSortV2.gc"; "par2.gc"];;

// Test of programs covered by the fifth task using optimized compilation (Section 8.2):
List.iter execOpt ["par1.gc"; "factImpPTyp.gc"; "QuickSortV2.gc"; "par2.gc"];;


*)
*)
 let p1Tree = parseFromFile "Pointer1.gc"

 let _ = tcP p1Tree;;

 let p1Code = CP p1Tree;; 

 let _ = goTrace p1Tree;;