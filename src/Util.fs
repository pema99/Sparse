namespace Sparse

module internal Util =
  open System
  
  let tryParseWrapper (func: string -> bool * _) param =
    match func param with
    | true, value -> Some (value :> obj)
    | _ -> None
  
  let parseInt = tryParseWrapper Int32.TryParse
  let parseFloat = tryParseWrapper Single.TryParse
  let parseDouble = tryParseWrapper Double.TryParse
  let parseBool = tryParseWrapper Boolean.TryParse
  let parseChar = tryParseWrapper Char.TryParse
  let parseUInt = tryParseWrapper UInt32.TryParse
  let parseByte = tryParseWrapper Byte.TryParse
  
  let parseToObject literalType literal =
    match literalType with
    | t when t = typeof<string> -> Some (literal :> obj)
    | t when t = typeof<int> -> parseInt literal
    | t when t = typeof<float> -> parseFloat literal 
    | t when t = typeof<double> -> parseDouble literal 
    | t when t = typeof<bool> -> parseBool literal
    | t when t = typeof<char> -> parseChar literal
    | t when t = typeof<uint32> -> parseUInt literal
    | t when t = typeof<byte> -> parseByte literal
    | _ -> None

  type Result<'T, 'U> with
    static member isError result =
      match result with
      | Ok(_) -> false
      | Error(_) -> true
    static member isOk result =
      not (Result.isError result)
    static member getOk result =
      match result with
      | Ok(res) -> res
      | Error(_) -> failwith "Result is Error"
    static member getError result =
      match result with
      | Ok(_) -> failwith "Result is Ok"
      | Error(res) -> res
    static member checkOk result =
      match result with
      | Ok(_) -> Some(result)
      | Error(_) -> None    
    static member checkError result =
      match result with
      | Ok(_) -> None
      | Error(_) -> Some(result)
