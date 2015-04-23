module PixelBlanket.Files
open PixelBlanket.Types
open System
open System.Collections.Generic
open System.IO
open System.Threading
let photosChangedEvent = new Event<PixelEvent> ()
let photosFolder = "/home/pi/Pictures"
let mutable photosList = []
let startFilePolling () =
    let syncContext = SynchronizationContext.CaptureCurrent ()
    let updatePhotosList () =
        let mutable sorted = []
        let directory = DirectoryInfo photosFolder
        if directory.Exists then
            for fileInfo in directory.EnumerateFiles () do
                if (fileInfo.FullName.ToLower ()).EndsWith (".jpg") then
                    sorted <- FileInfo2 (fileInfo.FullName, fileInfo.LastWriteTime) :: sorted
        sorted <- sorted |> List.sortBy (function | FileInfo2 (_, lastWriteTime) -> lastWriteTime) |> List.rev
        if photosList <> sorted then
            photosList <- sorted
            syncContext.RaiseEvent photosChangedEvent (PhotosChanged photosList)
    updatePhotosList ()
    async
        {
        while true do
            do! Async.Sleep (10 * 1000)
            updatePhotosList ()
        } |> Async.Start
    PhotosChanged photosList
let blackPixbuf = new Gdk.Pixbuf (Gdk.Colorspace.Rgb, true, 8, screenWidth, screenHeight)
let pixbufCache = new Dictionary<string, Gdk.Pixbuf> ()
let lru = new Dictionary<string, DateTime> ()
let maxCacheCount = 25
let getImagePixbuf (FileInfo2 (fileName, _)) =
    if not (pixbufCache.ContainsKey fileName) then
        if pixbufCache.Count >= maxCacheCount then
            let mutable oldestTime = DateTime.Now
            let mutable oldestName = ""
            for pair in lru do
                if pair.Value < oldestTime then
                    oldestTime <- pair.Value
                    oldestName <- pair.Key
            pixbufCache.[oldestName].Dispose ()
            pixbufCache.Remove oldestName |> ignore
            lru.Remove oldestName |>ignore
        pixbufCache.[fileName] <- new Gdk.Pixbuf (fileName)
    lru.[fileName] <- DateTime.Now
    pixbufCache.[fileName]
