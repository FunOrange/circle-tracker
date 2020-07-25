module JunUtils

open System
open System.Text.RegularExpressions
open System.Globalization

let tryParseWith (tryParseFunc: string -> bool * _) = tryParseFunc >> function
    | true, v    -> Some v
    | false, _   -> None

let isTypeOf (tryParseFunc: string -> bool * _) = tryParseFunc >> function
    | true, v  -> true
    | false, _ -> false

let tryParseInt     = tryParseWith System.Int32.TryParse
let tryParseSingle  = tryParseWith (fun num -> System.Single.TryParse(num, NumberStyles.AllowDecimalPoint ||| NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture) )
let tryParseDouble  = tryParseWith (fun num -> System.Double.TryParse(num, NumberStyles.AllowDecimalPoint ||| NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture) )
let tryParseDecimal = tryParseWith (fun num -> System.Decimal.TryParse(num, NumberStyles.AllowDecimalPoint ||| NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture) )
let tryParseBool input =
    match input with
    | "true" -> Some(true)
    | "false" -> Some(false)
    | "1" -> Some(true)
    | "0" -> Some(false)
    | _ -> None

// active patterns for converting strings to other data types
let (|Int|_|)       = tryParseInt
let (|Single|_|)    = tryParseSingle
let (|Double|_|)    = tryParseDouble
let (|Bool|_|)      = tryParseBool
let (|Decimal|_|)   = tryParseDecimal

let isInt     = isTypeOf System.Int32.TryParse
let isSingle  = isTypeOf System.Single.TryParse
let isDouble  = isTypeOf System.Double.TryParse
let isDecimal = isTypeOf System.Decimal.TryParse
let isBool    = isTypeOf System.Boolean.TryParse

let toBool str =
    match str with
    | "1" -> true
    | _ -> false

// other parsing functions
let (|Regex|_|) pattern input =
    let m = Regex.Match(input, pattern)
    if m.Success then Some(List.tail [ for g in m.Groups -> g.Value ])
    else None

let removeOuterQuotes (s:string) =
    match s with
    | Regex "^\"(.+)\"$" [inquotes] -> inquotes
    | _ -> s

let split separator (s:string) =
    let quoteCount = (Seq.filter ((=) '"') s) |> Seq.length
    let fixedStr =
        match quoteCount % 2 with
        | 1 -> s.Replace("\"", "")
        | _ -> s
    let values = ResizeArray<_>()
    let rec gather start i =
        let add () = fixedStr.Substring(start,i-start) |> values.Add
        if i = fixedStr.Length then add()
        elif fixedStr.[i] = '"' then inQuotes start (i+1) 
        elif fixedStr.[i] = separator then add(); gather (i+1) (i+1) 
        else gather start (i+1)
    and inQuotes start i =
        if fixedStr.[i] = '"' then gather start (i+1)
        else inQuotes start (i+1)
    gather 0 0

    values.ToArray()
    |> Array.toList
    |> List.map removeOuterQuotes

let parseCsv = split ','
let parseSpaceSeparatedList = split ' '

let tryParseCsvInt (str:string) : list<int> option = 
    let items = parseCsv str
    let validInts = List.fold (fun acc cur -> acc && (isInt cur)) true items
    match validInts with
    | true -> Some(List.map int items)
    | false -> None


// check if a list of strings match the expected types when casted
// actually don't need this... can just active pattern match on the list
let rec typesMatch vals types : bool = 
    match types with
    | t::ts -> 
        match t with
        | "int" ->
            match vals with
            | v::vs ->
                if (isInt v)
                then (typesMatch vs ts) // keep going!!!
                else false
            | _ -> false // out of values?
        | "decimal" -> 
            match vals with
            | v::vs ->
                if (isDecimal v)
                then (typesMatch vs ts) // keep going!!!
                else false
            | _ -> false // out of values?
        | _ ->
            match vals with
            | v::vs -> (typesMatch vs ts) // keep going!!!
            | _ -> false // out of values?
    | [] -> true // reached end, all checks passed


// print functions
let parseError obj =
    printfn "Error parsing %A" obj
    None

// parse an entire section
let parseSectionUsing parserfn lines = 
    let rec parseSectionUsing' parserfn lines result = 
        match lines with
        | head::tail ->
            match parserfn head with
            | Some(data) -> parseSectionUsing' parserfn tail (result @ [data])
            | None       -> parseSectionUsing' parserfn tail result
        | [] -> result
    parseSectionUsing' parserfn lines []

let multiply (i:int) (d:decimal) = int ( (decimal i) * d )
let divide (i:int) (d:decimal)   = int ( (decimal i) / d )
