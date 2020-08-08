namespace FsBeatmapProcessor

open JunUtils
open BeatmapParser
open General
open Editor
open Metadata
open Difficulty
open Events
open TimingPoints
open Colours
open HitObjects
open System
open System.IO

// C# Facing Interface 

type GameMode =
    | osu = 0
    | Taiko = 1
    | CatchtheBeat = 2
    | Mania = 3

[<Class; Sealed>]
type Beatmap(file, repr, cl) =

    member internal this.generalTryGetOr isfn getfn noneval =
        match this.changelist.general |> List.tryFind isfn with
        | Some x -> getfn x
        | None ->
            match this.originalFileRepresentation.general |> List.tryFind isfn with
            | Some x -> getfn x
            | None -> noneval
    
    member internal this.metadataTryGetOr isfn getfn noneval =
        match this.changelist.metadata |> List.tryFind isfn with
        | Some x -> getfn x
        | None ->
            match this.originalFileRepresentation.metadata |> List.tryFind isfn with
            | Some x -> getfn x
            | None -> noneval
    
    member internal this.difficultyTryGetOr isfn getfn noneval =
        match this.changelist.difficulty |> List.tryFind isfn with
        | Some x -> getfn x
        | None ->
            match this.originalFileRepresentation.difficulty |> List.tryFind isfn with
            | Some x -> getfn x
            | None -> noneval

    (* internal state/representation of beatmap *)
    member val public Filename = file with get, set
    member internal this.originalFileRepresentation = repr
    member val internal changelist = cl with get, set
    
    (* Public *)
    member public this.Valid with get() = (this.originalFileRepresentation.general <> [])

    // main constructor
    new(file) = 
        let changelist = {general=[]; editor=[]; metadata=[]; difficulty=[]; events=[]; timingPoints=[]; colours=[]; hitObjects=[]}
        Beatmap(file, parseBeatmapFile file, changelist)

    // copy constructor
    new(other:Beatmap) =
       Beatmap(other.Filename, other.originalFileRepresentation, other.changelist)

    member public this.Save() = this.Save(this.Filename)
    member public this.Save(outputPath) =
        let replaceOrAppend replaceCondition thingToInsert destlist =
            let mutable retlist = []
            let mutable replaceSuccess = false
            for candidate in destlist do
                if (replaceCondition candidate)
                then retlist <- retlist @ [thingToInsert]; replaceSuccess <- true
                else retlist <- retlist @ [candidate]
            if replaceSuccess
            then retlist
            else retlist @ [thingToInsert]

        let rec applyGeneralChanges changelist targetlist =
            match changelist with
            | change::changes ->
                match change with
                | AudioFilename _            -> applyGeneralChanges changes (replaceOrAppend isAudioFilename change targetlist)
                | AudioLeadIn _              -> applyGeneralChanges changes (replaceOrAppend isAudioLeadIn change targetlist)
                | PreviewTime _              -> applyGeneralChanges changes (replaceOrAppend isPreviewTime change targetlist)
                | Countdown _                -> applyGeneralChanges changes (replaceOrAppend isCountdown change targetlist)
                | SampleSet _                -> applyGeneralChanges changes (replaceOrAppend isSampleSet change targetlist)
                | StackLeniency _            -> applyGeneralChanges changes (replaceOrAppend isStackLeniency change targetlist)
                | Mode _                     -> applyGeneralChanges changes (replaceOrAppend isMode change targetlist)
                | LetterboxInBreaks _        -> applyGeneralChanges changes (replaceOrAppend isLetterboxInBreaks change targetlist)
                | UseSkinSprites _           -> applyGeneralChanges changes (replaceOrAppend isUseSkinSprites change targetlist)
                | OverlayPosition _          -> applyGeneralChanges changes (replaceOrAppend isOverlayPosition change targetlist)
                | SkinPreference _           -> applyGeneralChanges changes (replaceOrAppend isSkinPreference change targetlist)
                | EpilepsyWarning _          -> applyGeneralChanges changes (replaceOrAppend isEpilepsyWarning change targetlist)
                | CountdownOffset _          -> applyGeneralChanges changes (replaceOrAppend isCountdownOffset change targetlist)
                | SpecialStyle _             -> applyGeneralChanges changes (replaceOrAppend isSpecialStyle change targetlist)
                | WidescreenStoryboard _     -> applyGeneralChanges changes (replaceOrAppend isWidescreenStoryboard change targetlist)
                | SamplesMatchPlaybackRate _ -> applyGeneralChanges changes (replaceOrAppend isSamplesMatchPlaybackRate change targetlist)
                | _                          -> targetlist
            | [] -> targetlist

        let rec applyMetadataChanges changelist targetlist =
            match changelist with
            | change::changes ->
                match change with
                | Title _         -> applyMetadataChanges changes (replaceOrAppend isTitle change targetlist)
                | TitleUnicode _  -> applyMetadataChanges changes (replaceOrAppend isTitleUnicode change targetlist)
                | Artist _        -> applyMetadataChanges changes (replaceOrAppend isArtist change targetlist)
                | ArtistUnicode _ -> applyMetadataChanges changes (replaceOrAppend isArtistUnicode change targetlist)
                | Creator _       -> applyMetadataChanges changes (replaceOrAppend isCreator change targetlist)
                | Version _       -> applyMetadataChanges changes (replaceOrAppend isVersion change targetlist)
                | Source _        -> applyMetadataChanges changes (replaceOrAppend isSource change targetlist)
                | SearchTerms _   -> applyMetadataChanges changes (replaceOrAppend isSearchTerms change targetlist)
                | BeatmapID _     -> applyMetadataChanges changes (replaceOrAppend isBeatmapID change targetlist)
                | BeatmapSetID _  -> applyMetadataChanges changes (replaceOrAppend isBeatmapSetID change targetlist)
                | _               -> targetlist
            | [] -> targetlist

        let rec applyDifficultyChanges changelist targetlist = 
            match changelist with
            | change::changes ->
                match change with
                | HPDrainRate _       -> applyDifficultyChanges changes (replaceOrAppend isHPDrainRate change targetlist)
                | CircleSize _        -> applyDifficultyChanges changes (replaceOrAppend isCircleSize change targetlist)
                | OverallDifficulty _ -> applyDifficultyChanges changes (replaceOrAppend isOverallDifficulty change targetlist)
                | ApproachRate _      -> applyDifficultyChanges changes (replaceOrAppend isApproachRate change targetlist)
                | SliderMultiplier _  -> applyDifficultyChanges changes (replaceOrAppend isSliderMultiplier change targetlist)
                | SliderTickRate _    -> applyDifficultyChanges changes (replaceOrAppend isSliderTickRate change targetlist)
                | _                   -> targetlist
            | [] -> targetlist
            
        let exportGeneral      = applyGeneralChanges (List.rev this.changelist.general) this.originalFileRepresentation.general
        let exportEditor       = this.originalFileRepresentation.editor
        let exportMetadata     = applyMetadataChanges (List.rev this.changelist.metadata) this.originalFileRepresentation.metadata
        let exportDifficulty   = applyDifficultyChanges (List.rev this.changelist.difficulty) this.originalFileRepresentation.difficulty
        let exportEvents       = if this.changelist.events = []       then this.originalFileRepresentation.events       else this.changelist.events
        let exportTimingPoints = if this.changelist.timingPoints = [] then this.originalFileRepresentation.timingPoints else this.changelist.timingPoints
        let exportColours      = this.originalFileRepresentation.colours
        let exportHitObjects   = if this.changelist.hitObjects = []   then this.originalFileRepresentation.hitObjects   else this.changelist.hitObjects

        let exportFileLines = ["osu file format v14";""] @ (List.map generalInfoToString exportGeneral) @ (List.map editorSettingToString exportEditor) @ (List.map metadataToString exportMetadata) @ (List.map difficultySettingToString exportDifficulty) @ (List.map eventToString exportEvents) @ (List.map timingPointToString exportTimingPoints) @ (List.map colourSettingToString exportColours) @ (List.map hitObjectToString exportHitObjects)
        File.WriteAllLines(outputPath, exportFileLines, Text.Encoding.UTF8)
        ()
        
    member public this.RemoveSpinners() =
        let objects =
            if (this.changelist.hitObjects |> removeHitObjectComments) = [] then
                this.originalFileRepresentation.hitObjects
            else
                this.changelist.hitObjects

        let notSpinner = function
                         | Spinner _ -> false
                         | _         -> true
                        

        let newHitObjects = List.filter notSpinner objects

        this.changelist <- {this.changelist with hitObjects = newHitObjects}

    member public this.SetRate (rate:decimal) =
        let originalPreviewTime = match this.originalFileRepresentation.general |> List.tryFind isPreviewTime with
                                  | Some previewTime -> getPreviewTime previewTime
                                  | None -> 0
        let newPreviewTime = int ((1M / rate) * decimal (originalPreviewTime))
        let newGeneral = PreviewTime(newPreviewTime) :: this.changelist.general 

        let newEvents = this.originalFileRepresentation.events
                        |> List.map (function
                                     | Video b -> Video { b with startTime = (divide b.startTime rate)}
                                     | Break b -> Break { b with startTime = (divide b.startTime rate);
                                                                 endTime   = (divide b.endTime   rate)}
                                     | other -> other)

        let newTimingPoints = this.originalFileRepresentation.timingPoints
                              |> List.map (function
                                           | TimingPoint tp ->
                                               if tp.uninherited
                                               then (TimingPoint { tp with time       = divide tp.time rate;
                                                                           beatLength = tp.beatLength / rate })
                                               else (TimingPoint { tp with time       = divide tp.time rate })
                                           | comment -> comment)

        let newHitObjects = this.originalFileRepresentation.hitObjects
                            |> List.map (function
                                         | HitCircle c  -> HitCircle { c with time = (divide c.time rate) }
                                         | Slider s     -> HitCircle { s with time = (divide s.time rate) }
                                         | Spinner spin -> Spinner   { spin with time    = (divide spin.time rate);
                                                                                 endTime = (divide spin.endTime rate)}
                                         | Hold h       -> Hold      { h with time    = (divide h.time rate);
                                                                              endTime = (divide h.endTime rate)}
                                         | comment      -> comment)

        this.changelist <- {this.changelist with general      = newGeneral;
                                                 events       = newEvents;
                                                 timingPoints = newTimingPoints;
                                                 hitObjects   = newHitObjects; }

    // get song dominant bpm
    member public this.Bpm with get() =
        //printfn "@@@ bpm func:"
        if (this.originalFileRepresentation.hitObjects |> removeHitObjectComments   |> List.length) = 0 then
            //printfn "@@@ this.originalFileRepresentation.hitObjects length = 0, return 0"
            0M
        else if (this.originalFileRepresentation.timingPoints |> removeTimingPointComments |> List.length) = 0 then
            //printfn "@@@ this.originalFileRepresentation.timingPoints length = 0, return 0"
            0M
        else

        let rec beatLengthDurations (timingPoints:list<Tp>) lastObject : list<decimal * int> = 
            match timingPoints with
            | tp1::tp2::tps ->
                let duration =  tp2.time - tp1.time
                (tp1.beatLength, duration)::(beatLengthDurations timingPoints.Tail lastObject) 
                
            | [tp1] -> 
                // duration: start time -> last hit object
                let endtime =
                    match lastObject with
                    | HitCircle x -> x.time
                    | Slider x    -> x.time
                    | Spinner x   -> x.time
                    | Hold x      -> x.time
                    | _           -> 0
                let duration = endtime - tp1.time
                let duration' = if duration > 0 then duration else 0
                [(tp1.beatLength, duration')]
                
            | [] -> // shouldn't get here normally
                []

        // someone please teach me how to write f#
        let lastObject =
            if this.changelist.timingPoints = [] then
                this.originalFileRepresentation.hitObjects |> removeHitObjectComments |> List.rev |> List.head
            else
                this.changelist.hitObjects |> removeHitObjectComments |> List.rev |> List.head

        // printfn "@@@ last object: %s" (hitObjectToString lastObject)
        let timingpoints    = if this.changelist.timingPoints = [] then this.originalFileRepresentation.timingPoints else this.changelist.timingPoints
        //printfn "@@@ timing points: %A" timingpoints
        let bpmTimingPoints =
            timingpoints
            |> List.filter isTimingPoint
            |> List.map getTimingPoint
            |> List.filter (fun tp -> tp.uninherited)
        // printfn "@@@ bpmTimingPoints: %A" bpmTimingPoints

        if bpmTimingPoints = [] then
            // printfn "@@@ bpmTimingPoints is empty, return 0"
            0M
        else
        
        let durations       = beatLengthDurations bpmTimingPoints lastObject
        //printf "@@@ durations: %A" durations
        let grouped1        = List.groupBy (fun (beatlength, _) -> beatlength) durations
        //printfn "@@@ grouped1: %A" grouped1
        let grouped2        = grouped1 |> List.map (fun (bl', tupleList) -> (bl', List.map (fun (_, duration) -> duration) tupleList))
        // printfn "@@@ grouped2: %A" grouped2
        let groupedSums     = grouped2 |> List.map (fun (beatLength, durations) -> (beatLength, List.sum durations))
        // printfn "@@@ groupedSums: %A" groupedSums

        let (maxBeatLength, _) = List.maxBy (fun (beatLength, durationSum) -> durationSum) groupedSums
        // printfn "@@@ maxBeatLength: %A" maxBeatLength
        // printfn "return %A" (60000M / maxBeatLength)
        60000M / maxBeatLength
        
    member public this.MinBpm with get() =
        let bpmTimingPoints =
            (if this.changelist.timingPoints = [] then this.originalFileRepresentation.timingPoints else this.changelist.timingPoints)
            |> List.filter isTimingPoint
            |> List.map getTimingPoint
            |> List.filter (fun tp -> tp.uninherited)
        if bpmTimingPoints = [] then 0M else
        let maxBeatLengthTimingPoint = bpmTimingPoints |> List.maxBy (fun tp -> tp.beatLength)
        60000M / maxBeatLengthTimingPoint.beatLength

    member public this.MaxBpm with get() =
        let bpmTimingPoints =
            (if this.changelist.timingPoints = [] then this.originalFileRepresentation.timingPoints else this.changelist.timingPoints)
            |> List.filter isTimingPoint
            |> List.map getTimingPoint
            |> List.filter (fun tp -> tp.uninherited)
        if bpmTimingPoints = [] then 0M else
        let minBeatLengthTimingPoint = bpmTimingPoints |> List.minBy (fun tp -> tp.beatLength)
        60000M / minBeatLengthTimingPoint.beatLength


        
    (* General and Metadata *)

    member public this.AudioFilename
        with get()               = this.generalTryGetOr isAudioFilename getAudioFilename ""
        and  set(x:string)       = this.changelist <- {this.changelist with general=(AudioFilename(x)::this.changelist.general)}
            
    member public this.Mode
        with get()               = enum<GameMode>(this.generalTryGetOr isMode getMode 0)
        and  set(x:GameMode)          = this.changelist <- {this.changelist with general=(Mode(int x)::this.changelist.general)}
            
    member public this.Title
        with get()               = this.metadataTryGetOr isTitle getTitle ""
        and  set(x:string)       = this.changelist <- {this.changelist with metadata=(Title(x)::this.changelist.metadata)}
            
    member public this.TitleUnicode
        with get()               = this.metadataTryGetOr isTitleUnicode getTitleUnicode ""
        and  set(x:string)       = this.changelist <- {this.changelist with metadata=(TitleUnicode(x)::this.changelist.metadata)}
            
    member public this.Artist
        with get()               = this.metadataTryGetOr isArtist getArtist ""
        and  set(x:string)       = this.changelist <- {this.changelist with metadata=(Artist(x)::this.changelist.metadata)}
            
    member public this.ArtistUnicode
        with get()               = this.metadataTryGetOr isArtistUnicode getArtistUnicode ""
        and  set(x:string)       = this.changelist <- {this.changelist with metadata=(ArtistUnicode(x)::this.changelist.metadata)}

    member public this.Creator
        with get()               = this.metadataTryGetOr isCreator getCreator ""
        and  set(x:string)       = this.changelist <- {this.changelist with metadata=(Creator(x)::this.changelist.metadata)}
            
    member public this.Version
        with get()               = this.metadataTryGetOr isVersion getVersion ""
        and  set(x:string)       = this.changelist <- {this.changelist with metadata=(Metadata.Version(x)::this.changelist.metadata)}
            
    member public this.Source
        with get()               = this.metadataTryGetOr isSource getSource ""
        and  set(x:string)       = this.changelist <- {this.changelist with metadata=(Source(x)::this.changelist.metadata)}
            
    member public this.Tags
        with get()                      = ResizeArray<string> (this.metadataTryGetOr isSearchTerms getSearchTerms [])
        and  set(x:ResizeArray<string>) = this.changelist <- {this.changelist with metadata=(SearchTerms(Seq.toList x)::this.changelist.metadata)}
            
    member public this.BeatmapID
        with get()               = this.metadataTryGetOr isBeatmapID getBeatmapID 0
        and  set(x:int)          = this.changelist <- {this.changelist with metadata=(BeatmapID(x)::this.changelist.metadata)}
            

    (* Difficulty Settings *)
    member public this.HPDrainRate
        with get()             = this.difficultyTryGetOr isHPDrainRate getHPDrainRate -1M
        and  set(x:decimal)    = this.changelist <- {this.changelist with difficulty=(HPDrainRate(x)::this.changelist.difficulty)}

    member public this.CircleSize
        with get()             = this.difficultyTryGetOr isCircleSize getCircleSize -1M
        and  set(x:decimal)    = this.changelist <- {this.changelist with difficulty=(CircleSize(x)::this.changelist.difficulty)}
            
    member public this.OverallDifficulty
        with get()             = this.difficultyTryGetOr isOverallDifficulty getOverallDifficulty -1M
        and  set(x:decimal)    = this.changelist <- {this.changelist with difficulty=(OverallDifficulty(x)::this.changelist.difficulty)}
            
    member public this.ApproachRate
        with get()             = this.difficultyTryGetOr isApproachRate getApproachRate -1M
        and  set(x:decimal)    = this.changelist <- {this.changelist with difficulty=(ApproachRate(x)::this.changelist.difficulty)}
            
    member public this.SliderTickRate
        with get()             = this.difficultyTryGetOr isSliderTickRate getSliderTickRate -1M
        and  set(x:decimal)    = this.changelist <- {this.changelist with difficulty=(SliderTickRate(x)::this.changelist.difficulty)}


    (* Events *)
    member public this.Background
        with get() = 
            match this.originalFileRepresentation.events |> List.tryFind isBackground with
            | Some x -> (getBackgroundFilename x)
            | None -> ""

    (* Other *)
    member public this.HitObjectCount
        with get() = this.originalFileRepresentation.hitObjects
                     |> List.filter (function
                                     | Comment _ -> false
                                     | _         -> true)
                     |> List.length

    member public this.FirstHitObjectTime
        with get() =
            let hitObjects = this.originalFileRepresentation.hitObjects
                             |> List.filter (function
                                             | Comment _ -> false
                                             | _         -> true)
            match hitObjects.Head with
            | HitCircle o -> o.time
            | Slider o -> o.time
            | Spinner o -> o.time
            | Hold o -> o.time
            | _ -> 0
