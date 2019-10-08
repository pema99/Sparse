namespace Sparse

module internal Usage =
  open Microsoft.FSharp.Reflection
  open System.Collections.Generic
  open System

  open Util

  let buildUsageMessage (kind: Type) (name: string) (argMap: Map<string, UnionCaseInfo>) unique required =
    //Build dict arg -> string
    let argDict = new Dictionary<UnionCaseInfo, string list>()
    argMap 
      |> Map.iter (fun k v ->
        if argDict.ContainsKey v then
          argDict.[v] <- k :: argDict.[v]
        else
          argDict.[v] <- [k])

    //Get type metadata
    let methods = kind.GetMethods()
    let helpPrinter = methods |> Array.select (fun x -> x.Name = "get_Usage")
    let mutable args = ""
    let mutable options = "\n\nOptions:\n"

    //Build help message for each arg
    for kvp in argDict do
      let attrs = kvp.Key.GetCustomAttributes()
      let hidden = attrs |> Array.exists (fun x -> x :? Hidden)
      if not hidden then
        //Args
        let fields = kvp.Key.GetFields()        
        let param =
          if fields.Length > 0 then
            let paramName =
              match attrs |> Array.select (fun x -> x :? ParamName) with
              | Some(attr) -> (attr :?> ParamName).Name
              | None -> fields.[0].PropertyType.Name
            sprintf " <%s>" paramName
          else
            "" 
        let getRes =
          if attrs |> Array.exists (fun x -> x :? Required) then
            sprintf "%s%s "
          else
            sprintf "[%s%s] "
        let arg = kvp.Value |> List.rev |> String.concat ", "
        args <- args + getRes arg param

        //Options
        if Option.isSome helpPrinter then
          let defaultParams =
            if kvp.Key.GetFields().Length = 0 then [| |]
            else [| defaultObject(kvp.Key.GetFields().[0].PropertyType) |]
          let defaultUnionCase = FSharpValue.MakeUnion(kvp.Key, defaultParams)
          let helpString = (Option.get helpPrinter).Invoke(defaultUnionCase, [||]) :?> string
          options <- options + (arg |> sprintf "%-20s") + helpString + "\n"
        
    (sprintf "Usage: %s " name) + args + options
