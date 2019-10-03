namespace Sparse

module internal Parser =
  open System
  open Microsoft.FSharp.Reflection

  open Util

  let sugarLexeme (lexeme: string) =
    match lexeme.Length with
    | 1 -> "-" + lexeme
    | _ -> "--" + lexeme

  let augmentLexemes (arg: UnionCaseInfo) =
    (sugarLexeme (arg.Name.ToLower()), arg) :: 
    (arg.GetCustomAttributes() 
      |> List.ofArray
      |> List.filter (fun attr -> attr :? AltName)
      |> List.map (fun attr -> sugarLexeme (attr :?> AltName).Name, arg))

  let buildArgMap (args: Type) =
    FSharpType.GetUnionCases args 
      |> List.ofArray
      |> List.collect augmentLexemes
      |> Map.ofList

  let buildArgument (argMap: Map<string, UnionCaseInfo>) token param =
    if argMap.ContainsKey token then
      let fieldTypes = argMap.[token].GetFields()
      let takesParam = fieldTypes.Length > 0
      let field =
        match param with
        | Some(param) ->
          if takesParam then parseToObject fieldTypes.[0].PropertyType param
          else None
        | None -> None
      match field, takesParam with
      | Some(res), true -> Ok (FSharpValue.MakeUnion (argMap.[token], [| res |])), true
      | None, false -> Ok (FSharpValue.MakeUnion (argMap.[token], [||])), false
      | Some(res), false -> Error (sprintf "Unexpected parameter '%s' after '%s'" (res.ToString()) token), false
      | None, true -> Error (sprintf "Missing parameter after '%s'" token), true
    else Error (sprintf "Invalid argument '%s'" token), false

  let parse argMap (tokens: string []) =    
    let rec parseCont (tokens: string list) =
      match tokens with
      | arg::param::tail -> 
        let res, tookParam = buildArgument argMap arg (Some param)
        if tookParam then res :: parseCont tail
        else res :: parseCont (param :: tail)
      | arg::tail ->   
        let res, tookParam = buildArgument argMap arg None
        res :: parseCont tail
      | _ -> []
    let res = parseCont (List.ofArray tokens)
    if List.exists Result.isError res then
      Error
        [ for err in res do
          if err |> Result.isError then
            yield err |> Result.getError ]  
    else Ok (List.map Result.getOk res)
