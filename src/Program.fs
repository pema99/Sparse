module Main

open System

open Sparse

type Arguments = 
  | [<AltName("c")>] File of string
  | Gravity of bool
  | [<AltName("v")>] Verbose

[<EntryPoint>]
let main argv =
  let parser = new ArgParser<Arguments>()
  parser.Parse ("-v --file hello.txt --gravity true --file stuff".Split(" ")) |> printfn "%A" 
  0
