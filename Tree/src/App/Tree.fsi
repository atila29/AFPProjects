module Tree

type Tree<'a>

val design  : Tree<'a> -> Tree<'a * float>

val drawTreePS  : Tree<string * float> -> string
