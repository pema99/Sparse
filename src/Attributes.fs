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

//A literal argument with no lexeme
type NoLexeme() =
  inherit ArgumentAttribute()
