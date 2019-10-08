module Main

open System

open Sparse

type Arguments = 
  | [<AltName("c")>] File of string
  | [<Unique; ParamName("grav")>] Gravity of bool
  | [<AltName("v")>] Verbose
  | [<Hidden>] Debug

  member this.Usage =
    match this with
    | File(_) -> "Specify a specific file"
    | Gravity(_) -> "Turn on gravity"
    | Verbose -> "Make more verbose"
    | Debug -> "Turn on debug mode"

[<EntryPoint>]
let main argv =
  let parser = new ArgParser<Arguments>("TestApp")
  parser.Parse ("-v --file hello.txt --file stuff".Split(" ")) |> printfn "%A"
  parser.Help |> printfn "%s"
  0
