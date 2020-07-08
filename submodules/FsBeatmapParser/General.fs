module General

open JunUtils

type GeneralInfo = 
    | AudioFilename            of string
    | AudioLeadIn              of int
    | PreviewTime              of int
    | Countdown                of int
    | SampleSet                of string
    | StackLeniency            of decimal
    | Mode                     of int
    | LetterboxInBreaks        of bool
    | UseSkinSprites           of bool
    | OverlayPosition          of string
    | SkinPreference           of string
    | EpilepsyWarning          of bool
    | CountdownOffset          of int
    | SpecialStyle             of bool
    | WidescreenStoryboard     of bool
    | SamplesMatchPlaybackRate of bool
    | Comment                  of string

// isn't there an easier way to do this...
let isAudioFilename                       = function AudioFilename _ -> true                          | _ -> false
let isAudioLeadIn                         = function AudioLeadIn _ -> true                            | _ -> false
let isPreviewTime                         = function PreviewTime _ -> true                            | _ -> false
let isCountdown                           = function Countdown _ -> true                              | _ -> false
let isSampleSet                           = function SampleSet _ -> true                              | _ -> false
let isStackLeniency                       = function StackLeniency _ -> true                          | _ -> false
let isMode                                = function Mode _ -> true                                   | _ -> false
let isLetterboxInBreaks                   = function LetterboxInBreaks _ -> true                      | _ -> false
let isUseSkinSprites                      = function UseSkinSprites _ -> true                         | _ -> false
let isOverlayPosition                     = function OverlayPosition _ -> true                        | _ -> false
let isSkinPreference                      = function SkinPreference _ -> true                         | _ -> false
let isEpilepsyWarning                     = function EpilepsyWarning _ -> true                        | _ -> false
let isCountdownOffset                     = function CountdownOffset _ -> true                        | _ -> false
let isSpecialStyle                        = function SpecialStyle _ -> true                           | _ -> false
let isWidescreenStoryboard                = function WidescreenStoryboard _ -> true                   | _ -> false
let isSamplesMatchPlaybackRate            = function SamplesMatchPlaybackRate _ -> true               | _ -> false
let isComment                             = function Comment _ -> true                                | _ -> false
let getAudioFilename                      = function AudioFilename x -> x                       | _ -> ""
let getAudioLeadIn                        = function AudioLeadIn x -> x                         | _ -> 0
let getPreviewTime                        = function PreviewTime x -> x                         | _ -> 0
let getCountdown                          = function Countdown x -> x                           | _ -> 0
let getSampleSet                          = function SampleSet x -> x                           | _ -> ""
let getStackLeniency                      = function StackLeniency x -> x                       | _ -> 0M
let getMode                               = function Mode x -> x                                | _ -> 0
let getLetterboxInBreaks                  = function LetterboxInBreaks x -> x                   | _ -> false
let getUseSkinSprites                     = function UseSkinSprites x -> x                      | _ -> false
let getOverlayPosition                    = function OverlayPosition x -> x                     | _ -> ""
let getSkinPreference                     = function SkinPreference x -> x                      | _ -> ""
let getEpilepsyWarning                    = function EpilepsyWarning x -> x                     | _ -> false
let getCountdownOffset                    = function CountdownOffset x -> x                     | _ -> 0
let getSpecialStyle                       = function SpecialStyle x -> x                        | _ -> false
let getWidescreenStoryboard               = function WidescreenStoryboard x -> x                | _ -> false
let getSamplesMatchPlaybackRate           = function SamplesMatchPlaybackRate x -> x            | _ -> false
let getComment                            = function Comment x -> x                             | _ -> ""

let tryParseGeneralInfo line : GeneralInfo option =
    match line with
    | Regex @"(.+)\s?:\s?(.+)" [key; value] ->
        match key with
        | "AudioFilename"            -> Some(AudioFilename(value))
        | "AudioLeadIn"              -> Some(AudioLeadIn(int value))
        | "PreviewTime"              -> Some(PreviewTime(int value))
        | "Countdown"                -> Some(Countdown(int value))
        | "SampleSet"                -> Some(SampleSet(value))
        | "StackLeniency"            -> Some(StackLeniency(decimal value))
        | "Mode"                     -> Some(Mode(int value))
        | "LetterboxInBreaks"        -> Some(LetterboxInBreaks(toBool value))
        | "UseSkinSprites"           -> Some(UseSkinSprites(toBool value))
        | "OverlayPosition"          -> Some(OverlayPosition(value))
        | "SkinPreference"           -> Some(SkinPreference(value))
        | "EpilepsyWarning"          -> Some(EpilepsyWarning(toBool value))
        | "CountdownOffset"          -> Some(CountdownOffset(int value))
        | "SpecialStyle"             -> Some(SpecialStyle(toBool value))
        | "WidescreenStoryboard"     -> Some(WidescreenStoryboard(toBool value))
        | "SamplesMatchPlaybackRate" -> Some(SamplesMatchPlaybackRate(toBool value))
        | _ -> Some(Comment(line))
    | _ -> Some(Comment(line))

let generalInfoToString g =
    match g with
    | AudioFilename audiofilename    -> sprintf "AudioFilename: %s" audiofilename
    | AudioLeadIn leadin             -> sprintf "AudioLeadIn: %d" leadin
    | PreviewTime preview            -> sprintf "PreviewTime: %d" preview
    | Countdown countdown            -> sprintf "Countdown: %d" countdown
    | SampleSet ss                   -> sprintf "SampleSet: %s" ss
    | StackLeniency sl               -> sprintf "StackLeniency: %M" sl
    | Mode mode                      -> sprintf "Mode: %d" mode
    | LetterboxInBreaks lb           -> sprintf "LetterboxInBreaks: %d" (if lb then 1 else 0)
    | UseSkinSprites usess           -> sprintf "UseSkinSprites: %d" (if usess then 1 else 0)
    | OverlayPosition overlaypos     -> sprintf "OverlayPosition: %s" overlaypos
    | SkinPreference sp              -> sprintf "SkinPreference: %s" sp
    | EpilepsyWarning ew             -> sprintf "EpilepsyWarning: %d" (if ew then 1 else 0)
    | CountdownOffset co             -> sprintf "CountdownOffset: %d" co
    | SpecialStyle ss                -> sprintf "SpecialStyle: %d" (if ss then 1 else 0)
    | WidescreenStoryboard ws        -> sprintf "WidescreenStoryboard: %d" (if ws then 1 else 0)
    | SamplesMatchPlaybackRate smp   -> sprintf "SamplesMatchPlaybackRate: %d" (if smp then 1 else 0)
    | Comment comment                -> comment

let parseGeneralSection : string list -> GeneralInfo list = parseSectionUsing tryParseGeneralInfo
