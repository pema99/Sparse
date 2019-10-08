namespace Sparse

[<AutoOpen>]
module Lib =
  open System
  open Parser
  open Usage
  open Util

  type ArgParser<'T>(name: string) =
    let argMap = buildArgMap typeof<'T> 
    let unique, required = buildUniqueAndRequired typeof<'T>
    member this.Parse tokens =
      let res = parse argMap tokens
      if Result.isError res then res
      else 
        checkUniqueAndRequired unique required (Result.getOk res) 
    member this.Usage =
      buildUsageMessage typeof<'T> name argMap unique required
