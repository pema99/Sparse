namespace Sparse

module internal Parser =
  open System
  open Microsoft.FSharp.Reflection

  open Util

  //Add '-' or '--' to a lexeme depending on it's length
  let addPrefix (lexeme: string) =
    match lexeme.Length with
    | 1 -> "-" + lexeme
    | _ -> "--" + lexeme

  //Build a mapping table between lexeme strings and UnionCaseInfo instances
  let buildArgMap<'T> =
    //Build a list of tables entries for the mapping table given a UnionCaseInfo
    let buildTableEntries (arg: UnionCaseInfo) =
      (addPrefix (arg.Name.ToLower()), arg) :: 
      (arg.GetCustomAttributes()
        |> List.ofArray
        |> List.filter (fun attr -> attr :? AltName)
        |> List.map (fun attr -> addPrefix (attr :?> AltName).Name, arg))
    //Make entries for all union cases and build a map from the concatenated results
    FSharpType.GetUnionCases typeof<'T>
      |> Seq.collect buildTableEntries
      |> Map.ofSeq

  //Given a mapping table, a token and optionally a parameter,
  //construct a union case from the token and parameter using the mapping table.
  //Return a tuple of the union case wrapped in a result and a boolean indicating
  //whether the argument succesfully took a parameter.
  let buildArgument (argMap: Map<string, UnionCaseInfo>) token param =
    if argMap.ContainsKey token then
      let fieldTypes = argMap.[token].GetFields()
      let takesParam = fieldTypes.Length > 0
      let field =
        match param with
        | Some(param) when takesParam -> tryParseToObject fieldTypes.[0].PropertyType param
        | _ -> None
      match field, takesParam with
      | Some(res), true ->  Ok (FSharpValue.MakeUnion (argMap.[token], [| res |])), true
      | None, false ->      Ok (FSharpValue.MakeUnion (argMap.[token], [||])), false
      | Some(res), false -> Error (sprintf "Unexpected parameter '%s' after '%s'" (res.ToString()) token), false
      | None, true ->       Error (sprintf "Missing or invalid parameter after '%s'" token), true
    else Error (sprintf "Invalid argument '%s'" token), false

  //Iterates a string array and attempts to parse arguments
  //returns a Result containing either the a succesfully parsed
  //list of union cases corresponding to arguments, or
  //a list of strings containing errors messages.
  let parse<'T> argMap (tokens: string []) =    
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
      res
        |> List.filter Result.isError
        |> List.map Result.getError
        |> Error
    else
      res
        |> List.map Result.getOk
        |> List.map (fun x -> x :?> 'T)
        |> Ok

  //Given the type of a union and the type of an attribute
  //filter the union cases of the union to those containing
  //the given attribute.
  let filterArgumentsByAttribute<'T, 'U> =
    FSharpType.GetUnionCases typeof<'T>
      |> Array.filter (fun x ->
        x.GetCustomAttributes()
          |> Array.exists (fun y -> y :? 'U))
      |> List.ofArray

  //Find certain arguments as specified but a list of UnionCaseInfo
  //and make table of tuples where each tuple contains the arguments name
  //as well as all found occurences of it.
  let findArguments (targets: UnionCaseInfo list) args =
    targets
      |> List.map (fun target ->
        target.Name.ToLower(),
        args
          |> List.filter (fun arg ->
            (FSharpValue.GetUnionFields(arg, arg.GetType()) |> fst) = target))

  //Given a predicate which takes a list of found arguments
  //and returns a bool, as well as an string formatter for errors,
  //a list of arguments to find and the list of parsed arguments itself,
  //check if the predicate holds for and return a result accordingly.
  let checkArgumentConstraint pred errorFormatter targets args =
    let errors =
      findArguments targets args
        |> List.filter (snd >> pred)
    if errors.Length > 0 then
      errors
        |> List.map (fst >> errorFormatter)
        |> Error
    else Ok args

  //Checks for duplicate unique arguments
  let checkUnique targets args =
    checkArgumentConstraint
      (fun x -> x |> List.length > 1)
      (sprintf "Multiple occurences of unique argument '%s'")
      targets
      args

  //Checks for missing required arguments
  let checkRequired targets args =
    checkArgumentConstraint
      (fun x -> x |> List.length < 1)
      (sprintf "Missing required argument '%s'")
      targets
      args
