namespace Sparse

open System

type ArgumentAttribute() = 
  inherit Attribute()

//Required
type Required() = 
  inherit ArgumentAttribute()

//Alternate name(s)
type AltName(name: string) = 
  inherit ArgumentAttribute()
  member this.Name = name

//Only allowed once
type Unique() = 
  inherit ArgumentAttribute()

//Not shown in help message
type Hidden() = 
  inherit ArgumentAttribute()

//Main command without any lexeme
type MainCommand() =
  inherit ArgumentAttribute()

//Custom parameter name for help messages
type ParamName(name: string) =
  inherit ArgumentAttribute()
  member this.Name = name
