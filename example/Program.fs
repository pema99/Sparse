module Main

//Open the Sparse prelude as such
open Sparse

//To use the library, first define your arguments as a discriminated union.
//Union case name defines lexeme of argument.
//First union case field defines type of argument.
//No fields mean the argument is a flag.

//Support types are:
// - string
// - int
// - float
// - double
// - bool
// - char
// - uint32
// - byte

//You can also augment arguments with the following attributes:
//   Name                 Description
// - Required             This argument must always exist
// - AltName(string)      This argument can also be referred to using the specified string
// - Unique               This argument must only occur 1 or less times
// - ParamName(string)    Specify the parameter name for this argument, otherwise parameter type is used
// - Hidden               Do not show this argument in the usage/help message
// - NoLexeme             This argument has no lexeme and just matches a value

//You can also optionally provide a member function on the DU named "Usage".
//This function will then be used to build part of the usage message.
//The function should take no inputs and return a string based on the union case of the caller.

type Arguments = 
  | [<AltName("c")>] File of string
  | [<Unique; ParamName("update")>] Update of bool
  | Lines of int
  | [<AltName("v")>] Verbose
  | [<Hidden>] Debug

  member this.Usage =
    match this with
    | File _ -> "Chooses a specific file"
    | Update _ -> "Forces and update if true"
    | Lines _ -> "Specify number of lines in output"
    | Verbose _ -> "Make output more verbose"
    | Debug _ -> "Turn on debug mode"


[<EntryPoint>]
let main argv =
  //To use your defined union type, first create an ArgParser<'T> instance with
  //the union type passed as the type parameter and the name of the program passed
  //in the constructor.
  let parser = new ArgParser<Arguments>("ExampleApp")

  //ArgParser<'T> exposes 2 members:
  //Parse: string [] -> Result<'T list, string list>
  //Usage: string

  //Pass the parse function a string array of arguments (eg. command line arguments)
  //and it will either give you list of constructed union cases matching the parsed
  //argument or a string list of error messages.
  //Below are some examples:
  let tryParseArguments args =
    let args = parser.Parse args
    match args with
    | Ok(cases) -> printfn "Succesfully parsed: %A" cases
    | Error(errors) -> errors |> List.iter (printfn "Error: %s")
    printfn ""

  //This is fine usage
  printfn "=== First example: -v --file hello.txt --file stuff"
  tryParseArguments ("-v --file hello.txt --file stuff".Split(" "))

  //Error: Type mismatch
  printfn "=== Second example: -v --file hello.txt --lines true --file stuff"
  tryParseArguments ("-v --file hello.txt --lines true --file stuff".Split(" "))

  //Error: Duplicate unique argument
  printfn "=== Third example: -v --update true --update false"
  tryParseArguments ("-v --update true --update false".Split(" "))

  //Lastly, ArgParser.Usage can be used to print the automatically built usage message.
  parser.Usage |> printfn "=== Usage message:\n%s"

  0
