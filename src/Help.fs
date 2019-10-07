namespace Sparse

module internal Help =
  open Microsoft.FSharp.Reflection
  open System.Collections.Generic

  let buildHelpMessage name (argMap: Map<string, UnionCaseInfo>) unique required =
    let argDict = new Dictionary<UnionCaseInfo, string list>()
    argMap 
      |> Map.iter (fun k v ->
        if argDict.ContainsKey v then
          argDict.[v] <- k :: argDict.[v]
        else
          argDict.[v] <- [k])
    let mutable args = ""
    for kvp in argDict do
      let attrs = kvp.Key.GetCustomAttributes()
      let hidden = attrs |> Array.exists (fun x -> x :? Hidden)
      if not hidden then
        let fields = kvp.Key.GetFields()        
        let param =
          if fields.Length > 0 then
            let paramName =
              if attrs |> Array.exists (fun x -> x :? ParamName) then
                ((attrs |> Array.find (fun x -> x :? ParamName)) :?> ParamName).Name
              else
                fields.[0].PropertyType.Name
            sprintf " <%s>" paramName
          else
            "" 
        let getRes =
          if attrs |> Array.exists (fun x -> x :? Required) then
            sprintf "%s%s "
          else
            sprintf "[%s%s] "
        args <- args + getRes (kvp.Value |> List.rev |> String.concat ", ") param
    (sprintf "Usage: %s " name) + args
