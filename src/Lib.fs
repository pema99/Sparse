namespace Sparse

[<AutoOpen>]
module Lib =
  open System
  open Parser

  type ArgParser<'T>() =
    member this.Parse = (parse (buildArgMap typeof<'T>))
  
