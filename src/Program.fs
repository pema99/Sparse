module Main

open System

open Sparse

type Arguments = 
  | [<AltName("c")>] File of string
  | [<Unique; ParamName("grav")>] Gravity of bool
  | [<AltName("v")>] Verbose
  | [<Hidden>] Debug

[<EntryPoint>]
let main argv =
  let parser = new ArgParser<Arguments>("TestApp")
  parser.Parse ("-v --file hello.txt --file stuff".Split(" ")) |> printfn "%A"
  parser.Help |> printfn "%s"
  0
