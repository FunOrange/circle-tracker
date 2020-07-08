module Events

open JunUtils
open System

type Background = 
    {
        startTime : int;
        filename  : string;
        xOffset   : int;
        yOffset   : int;
    }

type Video =
    {
        startTime : int;
        filename  : string;
        xOffset   : int;
        yOffset   : int;
    }

type Break =
    {
        startTime : int;
        endTime   : int;
    }

type BeatmapEvent =
    | Background of Background
    | Video      of Video
    | Break      of Break
    | Comment    of String

let isBackground          = function Background _ -> true          | _ -> false
let getBackgroundFilename = function Background x -> x.filename    | _ -> ""

// Background syntax: 0,0,filename,xOffset,yOffset
let tryParseBackground vals : Background option =
    match vals with
    | ["0"; _; f; Int x; Int y] ->
        Some({
            Background.startTime = 0;
            filename             = f;
            xOffset              = x;
            yOffset              = y;
        })
    | ["0"; _; f;] -> 
        Some({
            Background.startTime = 0;
            filename             = f;
            xOffset              = 0;
            yOffset              = 0;
        })
    | _ -> None


// Video syntax: Video,startTime,filename,xOffset,yOffset
let tryParseVideo vals : Video option =
    match vals with
    | ["1"; Int s; f; Int x; Int y] | ["Video"; Int s; f; Int x; Int y] ->
        Some({
            Video.startTime = s;
            filename        = f;
            xOffset         = x;
            yOffset         = y;
        })
    | _ -> None


// Break syntax: 2,startTime,endTime
let tryParseBreak vals : Break option = 
    match vals with
    | ["2"; Int s; Int e;] | ["Break"; Int s; Int e;] ->
        Some({
            startTime = s;
            endTime   = e;
        })
    | _ -> None


let tryParseEvent line : BeatmapEvent option =
    let values = parseCsv line
    match values.[0] with

    // Background syntax: 0,0,filename,xOffset,yOffset
    | "0" ->
        match tryParseBackground values with
        | Some(bg) -> Some(Background(bg))
        | _        -> Some(Comment(line))

    // Video syntax: Video,startTime,filename,xOffset,yOffset
    | "1" | "Video" ->
        match tryParseVideo values with
        | Some(bg) -> Some(Video(bg))
        | _        -> Some(Comment(line))

    // Break syntax: 2,startTime,endTime
    | "2" ->
        match tryParseBreak values with
        | Some(br) -> Some(Break(br))
        | _        -> Some(Comment(line))

    | _ -> Some(Comment(line))

let eventToString ev =
    match ev with
    | Background bg   -> sprintf "0,0,\"%s\",%d,%d" bg.filename bg.xOffset bg.yOffset
    | Video vid       -> sprintf "Video,%d,\"%s\"" vid.startTime vid.filename
    | Break br        -> sprintf "2,%d,%d" br.startTime br.endTime
    | Comment comment -> comment

let parseEventSection : string list -> BeatmapEvent list = parseSectionUsing tryParseEvent
