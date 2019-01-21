// Michael R. Hansen 05-01-2016; Revised 04-01-2018


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

// Test of programs covered by the first task (Section 3.7):
List.iter exec ["Ex1.gc"; "Ex2.gc";"Ex3.gc"; "Ex4.gc"; "Ex5.gc"; "Ex6.gc"; "Skip.gc"];;

// Test of programs covered by the second task (Section 4.3):
List.iter exec ["Ex7.gc"; "fact.gc"; "factRec.gc"; "factCBV.gc"];;

// Test of programs covered by the fourth task (Section 5.4):
List.iter exec ["A0.gc"; "A1.gc"; "A2.gc"; "A3.gc"];;

// Test of programs covered by the fifth task (Section 6.1):
List.iter exec ["A4.gc"; "Swap.gc"; "QuickSortV1.gc"];;

// Test of programs covered by the fifth task (Section 7.4):
List.iter exec ["par1.gc"; "factImpPTyp.gc"; "QuickSortV2.gc"; "par2.gc"];;

// Custom tests

// pointers
let pointerTree = parseFromFile "pointer.gc";;
let _ = tcP pointerTree;;
let pointerCode = CP pointerTree;;
goTrace pointerTree

// Assesment
let assessmentTree = parseFromFile "AssessmentExample.gc";;
ignore (tcP assessmentTree)
let assessmentCode = CP assessmentTree;;
goTrace assessmentTree

// Extensions

// Ternary
let ternaryTree = parseFromFile "T0.gc";;
let _ = tcP ternaryTree;;
let ternaryCode = CP ternaryTree;;
goTrace ternaryTree

/// BELOW THIS POINT: each is expected to fail.
/// 
// Test of programs covered by the fifth task using optimized compilation (Section 8.2):
List.iter execOpt ["par1.gc"; "factImpPTyp.gc"; "QuickSortV2.gc"; "par2.gc"];;
let noReturnTree = parseFromFile "FunctionWithoutReturn.gc";;
let _ = tcP noReturnTree;;
let noReturnCode = CP noReturnTree;;
goTrace noReturnTree

ignore (parseFromFile "ProcedureWithReturn.gc")  // Parses successfully, but...
goTrace (parseFromFile "ProcedureWithReturn.gc") // BEWARE - DOES NOT TERMINATE!
