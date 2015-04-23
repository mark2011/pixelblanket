module PixelBlanket.Types

open Gtk
open Microsoft.FSharp.Reflection
open System
open System.Threading

let aspectWidth height = height * 16 / 9
let screenHeight = 1080
let screenWidth = aspectWidth screenHeight
let missingData msg = raise (new ApplicationException (sprintf "Missing data - %s" msg))
let invalidData msg = raise (new ApplicationException (sprintf "Invalid data - %s" msg))
let modulo n m = ((n % m) + m) % m
type SynchronizationContext with
    /// A standard helper extension method to raise an event on the GUI thread
    member syncContext.RaiseEvent (event: Event<_>) args =
        syncContext.Post ((fun _ -> event.Trigger args), state = null)
    /// A standard helper extension method to capture the current synchronization context.
    /// If none is present, use a context that executes work in the thread pool.
    static member CaptureCurrent () =
        match SynchronizationContext.Current with
        | null -> new SynchronizationContext ()
        | ctxt -> ctxt
let toString (x:'a) =
    match FSharpValue.GetUnionFields (x, typeof<'a>) with
    | case, _ -> case.Name
type TvPower = On | Off with
    member this.toString = toString this
type FileInfo2 = FileInfo2 of string * DateTime with
    member this.toString =
        match this with
        | FileInfo2 (name, lastWriteTime) -> sprintf "%s %s" name (lastWriteTime.ToString ())
type RemoteButton = OtherButton of string | Direction of int * int | Digit of int | Select | Rewind | FastForward | Play | Stop | Back with
    member this.toString =
        match this with
        | OtherButton n -> sprintf "OtherButton %s" n
        | Direction (x, y) -> sprintf "Direction %d %d" x y
        | Digit n -> sprintf "Digit %d" n
        | _ -> toString this
type TvInput = Tv | Hdmi of int with
    member this.toString =
        match this with
        | Hdmi n -> sprintf "Hdmi %d" n
        | _ -> toString this
type PixelEvent =
    | DrawingAreaExposed
    | DeleteWindow of DeleteEventArgs
    | Clock of DateTime
    | PhotosChanged of FileInfo2 list
    | TvPower of TvPower
    | TvInput of TvInput
    | RemoteButton of RemoteButton with
    member this.toString =
        match this with
        | Clock t -> "Clock " + (t.ToString ())
        | PhotosChanged photos ->
            sprintf "PhotosChanged length %A" (photos.Length)
        | TvPower power ->
            sprintf "TvPower %s" power.toString
        | TvInput input ->
            sprintf "TvInput %s" input.toString
        | RemoteButton button ->
            sprintf "RemoteButton %s" button.toString
        | _ -> toString this
