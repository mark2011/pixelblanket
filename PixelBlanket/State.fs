module PixelBlanket.State
open PixelBlanket.Types
open System
let rec getTime = function
    | Clock time :: _ -> time
    | _ :: tail       -> getTime tail
    | []              -> missingData "getTime"
let rec idle n = function
    | RemoteButton _ :: _     -> false
    | PhotosChanged _ :: _    -> false
    | Clock time :: _
        when time.Second = n  -> true
    | []                      -> true
    | _ :: tail               -> idle n tail
let quiet = idle 35
let rec enteredQuietly = function
    | [] -> false
    | Clock time :: tail when time.Second = 0 -> quiet tail
    | _ :: tail -> enteredQuietly tail
let chimeTime (time:DateTime) =
    modulo time.Minute 15 = 0
type CountingOrNot = Counting | NotCounting
let getScreen events =
    if chimeTime (getTime events) && idle 0 events && enteredQuietly events then
        ClockScreen
    else
        let rec getPhotoScreen countingOrNot index = function
            | [] -> NoPhotosScreen
            | event :: tail ->
                let add n = getPhotoScreen Counting (index + n) tail
                let stopCounting () = getPhotoScreen NotCounting index tail
                match countingOrNot, event with
                | _, PhotosChanged photos            -> PhotoScreen (List.nth photos (modulo index photos.Length))
                | NotCounting, _
                | _, RemoteButton Rewind             -> stopCounting ()
                | _, RemoteButton (Direction (n, 0)) -> add n
                | _, Clock time when
                       time.Second = 0
                    && quiet tail
                    && not (chimeTime time)          -> add 1
                | _, _                               -> add 0
        getPhotoScreen Counting 0 events
