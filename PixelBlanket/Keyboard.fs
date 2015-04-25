module PixelBlanket.Keyboard
open Gtk
open PixelBlanket.Types
open Serilog
let keyboardEvent = new Event<PixelEvent> ()
let startKeyboard (window:Window) =
    window.KeyPressEvent
    |> Event.add (fun e ->
        let f = keyboardEvent.Trigger
        match e.Event.Key with
        | Gdk.Key.q
        | Gdk.Key.Q    -> PixelQuit                        |> f
        | Gdk.Key.KP_4 -> RemoteButton (Direction (-1, 0)) |> f
        | Gdk.Key.KP_5 -> RemoteButton Select              |> f
        | Gdk.Key.KP_6 -> RemoteButton (Direction (+1, 0)) |> f
        | Gdk.Key.KP_7 -> RemoteButton Rewind              |> f
        | _ -> ())
