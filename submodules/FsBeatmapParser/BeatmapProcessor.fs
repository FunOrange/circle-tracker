module BeatmapParser

open System.IO
open JunUtils
open General
open Editor
open Metadata
open Difficulty
open Events
open TimingPoints
open Colours
open HitObjects

type BeatmapInternalRepresentation = 
    {
        general      : GeneralInfo       list;
        editor       : EditorSetting     list;
        metadata     : MetadataInfo      list;
        difficulty   : DifficultySetting list;
        events       : BeatmapEvent      list;
        timingPoints : TimingPoint       list;
        colours      : ColourSetting     list;
        hitObjects   : HitObject         list;
    }

type Section = 
    | General
    | Editor
    | Metadata
    | Difficulty
    | Events
    | TimingPoints
    | Colours
    | HitObjects

let isSectionHeader line =
    match line with
    | Regex "^\[(.+)\]" [header] ->
        match header with
        | "General"      -> true
        | "Editor"       -> true
        | "Metadata"     -> true
        | "Difficulty"   -> true
        | "Events"       -> true
        | "TimingPoints" -> true
        | "Colours"      -> true
        | "HitObjects"   -> true
        | _ -> false
    | _ -> false


let splitSections (fileLines:list<string>) : list<list<string>> =

    // get list of line numbers where a section header is placed
    let mutable headerIndices = []
    for i = 0 to (fileLines.Length - 1) do
        if isSectionHeader fileLines.[i] then
            headerIndices <- headerIndices @ [i]

    let rec getSections (sectionDividers:list<int>) (remainingLines:list<string>) : list<list<string>> =
        match sectionDividers with
        | sd1::sd2::sds ->
            remainingLines.[sd1..sd2-1] :: (getSections (sd2::sds) remainingLines)
        | [lastsd] -> [remainingLines.[lastsd..]]
        | _ -> [] // file contains no sections headers?

    getSections headerIndices fileLines


let whichSection (sectionLines:list<string>) : Section =
    assert (sectionLines.Length > 0)
    match sectionLines.[0] with 
    | "[General]"      -> General
    | "[Editor]"       -> Editor
    | "[Metadata]"     -> Metadata
    | "[Difficulty]"   -> Difficulty
    | "[Events]"       -> Events
    | "[TimingPoints]" -> TimingPoints
    | "[Colours]"      -> Colours
    | "[HitObjects]"   -> HitObjects
    | _ -> assert false; General // should never happen


let parseSections (sections: list<list<string>> ) =

    let headerIs headerName (section: string list ) =
        section.[0] = (sprintf "[%s]" headerName)

    let consolidatedSections = 
        [
            sections |> List.filter (headerIs "General")      |> List.concat;
            sections |> List.filter (headerIs "Editor")       |> List.concat;
            sections |> List.filter (headerIs "Metadata")     |> List.concat;
            sections |> List.filter (headerIs "Difficulty")   |> List.concat;
            sections |> List.filter (headerIs "Events")       |> List.concat;
            sections |> List.filter (headerIs "TimingPoints") |> List.concat;
            sections |> List.filter (headerIs "Colours")      |> List.concat;
            sections |> List.filter (headerIs "HitObjects")   |> List.concat;
        ]
    let ret = {
        general      = parseGeneralSection     consolidatedSections.[0];
        editor       = parseEditorSection      consolidatedSections.[1];
        metadata     = parseMetadataSection    consolidatedSections.[2];
        difficulty   = parseDifficultySection  consolidatedSections.[3];
        events       = parseEventSection       consolidatedSections.[4];
        timingPoints = parseTimingPointSection consolidatedSections.[5];
        colours      = parseColourSection      consolidatedSections.[6];
        hitObjects   = parseHitObjectSection   consolidatedSections.[7];
    }

    let tps = ret.timingPoints |> List.filter isTimingPoint
    let comments = ret.timingPoints |> List.filter (fun x -> not (isTimingPoint x))

    //printfn "################################################"
    //printfn "Parsed Timing Points:"
    //printfn "TimingPoints: %d" (List.length tps)
    //List.iter (fun tp -> printfn "%s" (timingPointToString tp)) tps
    //printfn "\n"
    //printfn "Comments: %d" (List.length comments)
    //List.iter (fun comment -> printfn "'%s'" (timingPointToString comment)) comments
    //printfn "################################################"
    //printfn ""
    ret


        
let parseBeatmapFile filename =
    //printfn "################################################"
    //printfn ""
    //printfn "Parsing file:"
    //printfn "%s" filename
    //printfn ""
    //printfn "################################################"

    File.ReadAllLines filename
    |> Array.toList
    |> splitSections
    |> parseSections


