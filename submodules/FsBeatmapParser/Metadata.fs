module Metadata

open JunUtils
open System

type MetadataInfo = 
    | Title         of string
    | TitleUnicode  of string
    | Artist        of string
    | ArtistUnicode of string
    | Creator       of string
    | Version       of string
    | Source        of string
    | SearchTerms   of list<string> // Tags
    | BeatmapID     of int
    | BeatmapSetID  of int
    | Comment       of string

// isn't there an easier way to do this...
let isTitle          = function Title _ -> true         | _ -> false
let isTitleUnicode   = function TitleUnicode _ -> true  | _ -> false
let isArtist         = function Artist _ -> true        | _ -> false
let isArtistUnicode  = function ArtistUnicode _ -> true | _ -> false
let isCreator        = function Creator _ -> true       | _ -> false
let isVersion        = function Version _ -> true       | _ -> false
let isSource         = function Source _ -> true        | _ -> false
let isSearchTerms    = function SearchTerms _ -> true   | _ -> false
let isBeatmapID      = function BeatmapID _ -> true     | _ -> false
let isBeatmapSetID   = function BeatmapSetID _ -> true  | _ -> false
let isComment        = function Comment _ -> true       | _ -> false
let getTitle         = function Title x -> x            | _ -> ""
let getTitleUnicode  = function TitleUnicode x -> x     | _ -> ""
let getArtist        = function Artist x -> x           | _ -> ""
let getArtistUnicode = function ArtistUnicode x -> x    | _ -> ""
let getCreator       = function Creator x -> x          | _ -> ""
let getVersion       = function Version x -> x          | _ -> ""
let getSource        = function Source x -> x           | _ -> ""
let getSearchTerms   = function SearchTerms x -> x      | _ -> []
let getBeatmapID     = function BeatmapID x -> x        | _ -> 0
let getBeatmapSetID  = function BeatmapSetID x -> x     | _ -> 0
let getComment       = function Comment x -> x          | _ -> ""

let tryParseMetadataField line : MetadataInfo option =
    match line with
    | Regex @"(.+?)\s?:\s?(.*)" [key; value] ->
        match key with
        | "Title"         -> Some(Title( value ))
        | "TitleUnicode"  -> Some(TitleUnicode( value ))
        | "Artist"        -> Some(Artist( value ))
        | "ArtistUnicode" -> Some(ArtistUnicode( value ))
        | "Creator"       -> Some(Creator( value ))
        | "Version"       -> Some(Version( value ))
        | "Source"        -> Some(Source( value ))
        | "Tags"          -> Some(SearchTerms( parseSpaceSeparatedList value ))
        | "BeatmapID"     -> Some(BeatmapID( int value ))
        | "BeatmapSetID"  -> Some(BeatmapSetID( int value ))
        | _ -> Some(Comment(line))
    | _ -> Some(Comment(line))

let metadataToString m = 
    match m with
    | Title t         -> sprintf "Title:%s" t
    | TitleUnicode t  -> sprintf "TitleUnicode:%s" t
    | Artist a        -> sprintf "Artist:%s" a
    | ArtistUnicode a -> sprintf "ArtistUnicode:%s" a
    | Creator c       -> sprintf "Creator:%s" c
    | Version v       -> sprintf "Version:%s" v
    | Source s        -> sprintf "Source:%s" s
    | SearchTerms s   -> sprintf "Tags:%s" (String.Join(" ", s))
    | BeatmapID b     -> sprintf "BeatmapID:%d" b
    | BeatmapSetID b  -> sprintf "BeatmapSetID:%d" b
    | Comment c       -> c

let parseMetadataSection : string list -> MetadataInfo list = parseSectionUsing tryParseMetadataField
