module PixelBlanket.State
open PixelBlanket.Types
let rec getTime =
    function
    | Clock time :: _ -> time
    | _ :: tail -> getTime tail
    | [] -> missingData "getTime"
let rec getPhotos2 accumulatedLeftAndRightButtons =
    function
    | PhotosChanged photos :: tail -> photos, getTime tail, accumulatedLeftAndRightButtons
    | _ :: tail -> getPhotos2 accumulatedLeftAndRightButtons tail
    | [] -> missingData "getPhotos2"
let rec getPhotos accumulatedLeftAndRightButtons =
    function
    | PhotosChanged photos :: tail -> getPhotos2 accumulatedLeftAndRightButtons (PhotosChanged photos :: tail)
    | RemoteButton Rewind :: tail -> getPhotos2 accumulatedLeftAndRightButtons tail
    | RemoteButton (Direction (n, 0)) :: tail -> getPhotos (accumulatedLeftAndRightButtons + n) tail
    | _ :: tail -> getPhotos accumulatedLeftAndRightButtons tail
    | [] -> missingData "getPhotos"
