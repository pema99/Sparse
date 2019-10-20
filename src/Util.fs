namespace Sparse

module internal Util =
  open System

  //Given a TryParse function, create a function wrapper for it 
  let tryParseWrapper (func: string -> bool * _) param =
    match func param with
    | true, value -> Some (value :> obj)
    | _ -> None

  //Functional TryParse wrappers
  let tryParseInt = tryParseWrapper Int32.TryParse
  let tryParseFloat = tryParseWrapper Single.TryParse
  let tryParseDouble = tryParseWrapper Double.TryParse
  let tryParseBool = tryParseWrapper Boolean.TryParse
  let tryParseChar = tryParseWrapper Char.TryParse
  let tryParseUInt = tryParseWrapper UInt32.TryParse
  let tryParseByte = tryParseWrapper Byte.TryParse

  //Given a type, and a string, dynamically parse the string
  //and cast it to the type, if possible, and return the result
  //upcasted to obj, wrapped in an option.
  let tryParseToObject literalType literal =
    match literalType with
    | t when t = typeof<string> -> Some (literal :> obj)
    | t when t = typeof<int> ->    tryParseInt literal
    | t when t = typeof<float> ->  tryParseFloat literal 
    | t when t = typeof<double> -> tryParseDouble literal 
    | t when t = typeof<bool> ->   tryParseBool literal
    | t when t = typeof<char> ->   tryParseChar literal
    | t when t = typeof<uint32> -> tryParseUInt literal
    | t when t = typeof<byte> ->   tryParseByte literal
    | _ -> None

  //Given a type, dynamically construct the default value for it.
  //Throws an exception if the type is not supported.
  let defaultObject objectType =
    match objectType with
    | t when t = typeof<string> -> "" :> obj 
    | t when t = typeof<int> ->    0 :> obj
    | t when t = typeof<float> ->  (float 0.0) :> obj
    | t when t = typeof<double> -> (double 0.0) :> obj 
    | t when t = typeof<bool> ->   false :> obj
    | t when t = typeof<char> ->   ' ' :> obj
    | t when t = typeof<uint32> -> (uint32 0) :> obj
    | t when t = typeof<byte> ->   (byte 0) :> obj
    | _ -> objectType.Name
             |> sprintf "Unsupported field type '%A'"
             |> failwith

  //Result type extensions
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
    static member tryGetOk result =
      match result with
      | Ok(_) -> Some(result)
      | Error(_) -> None    
    static member tryGetError result =
      match result with
      | Ok(_) -> None
      | Error(_) -> Some(result)
