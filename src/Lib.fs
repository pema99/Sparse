namespace Sparse

[<AutoOpen>]
module Lib =
  open System
  open Parser
  open Usage
  open Util

  type ArgParser<'T>(name: string) =
    let argMap = buildArgMap<'T> 
    let unique = filterArgumentsByAttribute<'T, Unique>
    let required = filterArgumentsByAttribute<'T, Required>
    member this.Parse tokens =
      let res = parse<'T> argMap tokens
      res
        |> Result.bind (checkUnique unique)
        |> Result.bind (checkRequired required)
    member this.Usage =
      buildUsageMessage typeof<'T> name argMap unique required
