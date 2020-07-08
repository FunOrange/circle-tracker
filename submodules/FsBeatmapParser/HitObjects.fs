module HitObjects

open System
open JunUtils

type ObjNoEndTime =
    {
        x          : int;
        y          : int;
        time       : int;
        typeval    : int;
        hitSound   : int;
        remainder  : string; // just don't bother trying to parse everything else
    }

type ObjWithEndTime = 
    {
        x          : int;
        y          : int;
        time       : int;
        typeval    : int;
        hitSound   : int;
        endTime    : int;
        remainder  : string; // just don't bother trying to parse everything else
    }


type HitObject = 
    | HitCircle of ObjNoEndTime
    | Slider    of ObjNoEndTime
    | Spinner   of ObjWithEndTime
    | Hold      of ObjWithEndTime
    | Comment   of string

let isNotHitObjectComment hobj =
    match hobj with
    | Comment _ -> false
    | _ -> true

let removeHitObjectComments (objs:list<HitObject>) = (List.filter isNotHitObjectComment objs)
    

let tryParseObjNoEndTime vals : ObjNoEndTime option =
    match vals with
    | x::y::ti::ty::hs::rest ->
        if (typesMatch [x;y;ti;ty;hs] ["int";"int";"int";"int";"int"]) then
            Some({
                x           = int x;
                y           = int y;
                time        = int ti;
                typeval     = int ty;
                hitSound    = int hs;
                remainder   = String.Join(",", rest);
            })
        else parseError vals
    | _ -> None
    

let tryParseSpinner vals : ObjWithEndTime option =
    match vals with
    | x::y::ti::ty::hs::et::rest ->
        if (typesMatch [x;y;ti;ty;hs;et] ["int";"int";"int";"int";"int";"int"]) then
            Some({
                x         = int x;
                y         = int y;
                time      = int ti;
                typeval   = int ty;
                endTime   = int et;
                hitSound  = int hs;
                remainder = String.Join(",", rest);
            })
        else parseError vals
    | _ -> None
    

let tryParseHold vals : ObjWithEndTime option =
    match vals with
    | x::y::ti::ty::hs::endtimeHitsample::rest ->
        if (typesMatch [x;y;ti;ty;hs] ["int";"int";"int";"int";"int"]) then
            match endtimeHitsample with
            | Regex "^(\d+):(.+)" [endtime; remainder] -> 
                Some({
                    x         = int x;
                    y         = int y;
                    time      = int ti;
                    typeval   = int ty;
                    hitSound  = int hs;
                    endTime   = int endtime;
                    remainder = remainder + String.Join(",", rest);
                })
            | _ -> parseError vals
        else parseError vals
    | _ -> None
    

let tryParseHitObject line : HitObject option =
    let vals = parseCsv line 
    match vals with
    | _::_::_::typeval::rest -> 
        match int typeval with

        // bit 0 high => HitCircle
        | typebyte when (typebyte &&& 1) <> 0 ->
            match tryParseObjNoEndTime vals with
            | Some(obj) -> Some(HitCircle(obj))
            | None -> Some(Comment(line))
        
        // bit 1 high => HitCircle
        | typebyte when (typebyte &&& 2) <> 0 ->
            match tryParseObjNoEndTime vals with
            | Some(obj) -> Some(Slider(obj))
            | None -> Some(Comment(line))
        
        // bit 3 high => Spinner
        | typebyte when (typebyte &&& 8) <> 0 ->
            match tryParseSpinner vals with
            | Some(obj) -> Some(Spinner(obj))
            | None -> Some(Comment(line))

        // bit 7 high => osu mania hold
        | typebyte when (typebyte &&& 128) <> 0 ->
            match tryParseHold vals with
            | Some(obj) -> Some(Hold(obj))
            | None -> Some(Comment(line))

        | _ -> Some(Comment(line))
    | _ -> Some(Comment(line))

let commaString r =
    if r = ""
    then ""
    else "," + r

let hitObjectToString obj = 
    match obj with
    | HitCircle c     -> sprintf "%d,%d,%d,%d,%d%s"    c.x c.y c.time c.typeval c.hitSound (commaString c.remainder)
    | Slider s        -> sprintf "%d,%d,%d,%d,%d%s"    s.x s.y s.time s.typeval s.hitSound (commaString s.remainder)
    | Spinner s       -> sprintf "%d,%d,%d,%d,%d,%d%s" s.x s.y s.time s.typeval s.hitSound s.endTime (commaString s.remainder)
    | Hold h          -> sprintf "%d,%d,%d,%d,%d,%d:%s" h.x h.y h.time h.typeval h.hitSound h.endTime h.remainder
    | Comment comment -> comment

let parseHitObjectSection : string list -> HitObject list = parseSectionUsing tryParseHitObject
