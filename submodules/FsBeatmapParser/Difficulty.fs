module Difficulty

open JunUtils

type DifficultySetting = 
    | HPDrainRate       of decimal
    | CircleSize        of decimal
    | OverallDifficulty of decimal
    | ApproachRate      of decimal
    | SliderMultiplier  of decimal
    | SliderTickRate    of decimal
    | Comment           of string

// isn't there an easier way to do this...
let isHPDrainRate         = function HPDrainRate _ -> true       | _ -> false
let isCircleSize          = function CircleSize _ -> true        | _ -> false
let isOverallDifficulty   = function OverallDifficulty _ -> true | _ -> false
let isApproachRate        = function ApproachRate _ -> true      | _ -> false
let isSliderMultiplier    = function SliderMultiplier _ -> true  | _ -> false
let isSliderTickRate      = function SliderTickRate _ -> true    | _ -> false
let isComment             = function Comment _ -> true           | _ -> false
let getHPDrainRate        = function HPDrainRate x -> x          | _ -> 0M
let getCircleSize         = function CircleSize x -> x           | _ -> 0M
let getOverallDifficulty  = function OverallDifficulty x -> x    | _ -> 0M
let getApproachRate       = function ApproachRate x -> x         | _ -> 0M
let getSliderMultiplier   = function SliderMultiplier x -> x     | _ -> 0M
let getSliderTickRate     = function SliderTickRate x -> x       | _ -> 0M
let getComment            = function Comment x -> x              | _ -> ""

let tryParseDifficultyOption line : DifficultySetting option =
    match line with
    | Regex @"(.+?)\s?:\s?(.*)" [key; value] ->
        match key with
        | "HPDrainRate"       -> Some(HPDrainRate( decimal value ))
        | "CircleSize"        -> Some(CircleSize( decimal value ))
        | "OverallDifficulty" -> Some(OverallDifficulty( decimal value ))
        | "ApproachRate"      -> Some(ApproachRate( decimal value ))
        | "SliderMultiplier"  -> Some(SliderMultiplier( decimal value ))
        | "SliderTickRate"    -> Some(SliderTickRate( decimal value ))
        | _ -> Some(Comment(line))
    | _ -> Some(Comment(line))

let difficultySettingToString ds = 
    match ds with
    | HPDrainRate hp       -> sprintf "HPDrainRate:%M" hp
    | CircleSize cs        -> sprintf "CircleSize:%M" cs
    | OverallDifficulty od -> sprintf "OverallDifficulty:%M" od
    | ApproachRate ar      -> sprintf "ApproachRate:%M" ar
    | SliderMultiplier sm  -> sprintf "SliderMultiplier:%M" sm
    | SliderTickRate tick  -> sprintf "SliderTickRate:%M" tick
    | Comment comment      -> comment


let parseDifficultySection : string list -> DifficultySetting list = parseSectionUsing tryParseDifficultyOption
