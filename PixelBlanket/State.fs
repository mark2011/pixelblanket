module PixelBlanket.State
open PixelBlanket.Types
let rec getTime = function
    | Clock time :: _ -> time
    | _ :: tail       -> getTime tail
    | []              -> missingData "getTime"
let rec idle = function
    | RemoteButton _ :: _     -> false
    | PhotosChanged _ :: _    -> false
    | Clock time :: _
        when time.Second = 35 -> true
    | []                      -> true
    | _ :: tail               -> idle tail
let getPhoto =
    let rec getPhoto' counting index = function
        | PhotosChanged photos :: tail                    -> Some (List.nth photos (modulo index photos.Length))
        | RemoteButton Rewind :: tail when counting       -> getPhoto' false index tail
        | RemoteButton (Direction (n, 0)) :: tail
            when counting                                 -> getPhoto' true (index + n) tail
        | Clock time :: tail
            when counting && time.Second = 0 && idle tail -> getPhoto' true (index + 1) tail
        | _ :: tail                                       -> getPhoto' counting index tail
        | []                                              -> None
    getPhoto' true 0
