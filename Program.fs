open System
open Microsoft.FSharp.Reflection
open System

type ArgumentAttribute() = 
    inherit Attribute()
type Required() = 
    inherit ArgumentAttribute()
type AltName(name: string) = 
    inherit ArgumentAttribute()
    member this.Name = name
type Unique() = 
    inherit ArgumentAttribute()
type Hidden() = 
    inherit ArgumentAttribute()
type NoLexeme() =
    inherit ArgumentAttribute()

type Arguments = 
    | [<AltName("c")>] File of string
    | Gravity of int
    | [<AltName("v")>] Verbose

[<EntryPoint>]
let main argv =
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
    
    let argMap =
        FSharpType.GetUnionCases typeof<Arguments> 
            |> List.ofArray
            |> List.collect augmentLexemes
            |> Map.ofList

    let tryParse func param =
        match func param with
        | true, value -> Some (value :> obj)
        | _ -> None

    let parseInt = tryParse Int32.TryParse
    let parseFloat = tryParse Single.TryParse
    let parseDouble = tryParse Double.TryParse
    let parseBool = tryParse Boolean.TryParse
    let parseChar = tryParse Char.TryParse
    let parseUInt = tryParse UInt32.TryParse
    let parseByte = tryParse Byte.TryParse

    let buildArgument token param =
        if argMap.ContainsKey token then
            let fieldTypes = argMap.[token].GetFields()
            let takesParam = fieldTypes.Length > 0
            let field = 
                if takesParam then
                    match fieldTypes.[0].PropertyType with
                    | t when t = typeof<string> -> Some (param :> obj)
                    | t when t = typeof<int> -> parseInt param
                    | t when t = typeof<float> -> parseFloat param 
                    | t when t = typeof<double> -> parseDouble param 
                    | t when t = typeof<bool> -> parseBool param
                    | t when t = typeof<char> -> parseChar param
                    | t when t = typeof<uint32> -> parseUInt param
                    | t when t = typeof<byte> -> parseByte param
                    | _ -> None
                else None
            match field, takesParam with
            | Some(res), true -> Some (FSharpValue.MakeUnion (argMap.[token], [| res |])), true
            | None, false -> Some (FSharpValue.MakeUnion (argMap.[token], [||])), false
            | _ -> None, false
        else None, false
    
    let parse (tokens: string []) =        
        let rec parseCont (tokens: string list) =
            match tokens with
            | arg::param::tail -> 
                let res, tookParam = buildArgument arg param
                if tookParam then res :: parseCont tail
                else res :: parseCont (param :: tail)
            | _ -> []
        let res = parseCont (List.ofArray tokens)
        if List.exists Option.isNone res then None
        else Some (List.map Option.get res)
    
    parse ("-v --file hello.txt --gravity 5".Split(" ")) |> printfn "%A"

    System.Console.ReadKey() |> ignore
    0
