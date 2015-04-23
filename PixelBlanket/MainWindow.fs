module PixelBlanket.MainWindow
open Gtk
open PixelBlanket.CecClient
open PixelBlanket.Clock
open PixelBlanket.Files
open PixelBlanket.GtkSupport
open PixelBlanket.Markup
open PixelBlanket.State
open PixelBlanket.Types
open Serilog
open System
open System.Text
open System.Threading
let display (photos:FileInfo2 list) index0 (t0:DateTime) (t1:DateTime) =
    if photos = [] then
        blackPixbuf, "\n\n\n\n\nPlace 1920x1080 jpg files\n   in folder /home/pi/Pictures\n\n\n" |> markup
    else
        let pixbuf =
            let photoIndex = modulo (index0 + t1.Minute - t0.Minute) photos.Length
            List.nth photos photoIndex |> getImagePixbuf
        let photoMarkup =
            (if t1.Second < 10 then lowerRight 10 (t1.ToString "h:mm tt") else "") |> markup
        pixbuf, photoMarkup
type MyWindow () as this = 
    inherit Window ("MainWindow")
    let drawingArea = new DrawingArea ()
    do
        let log = (((new LoggerConfiguration ())
            .WriteTo.ColoredConsole ())
            .WriteTo.RollingFile "/home/pi/pixelblanket-log-{date}.txt").CreateLogger ()
        Serilog.Log.Logger <- log
        this.ModifyBg (StateType.Normal, Gdk.Color.Zero)
        this.Add drawingArea
        this.ShowAll ()
        this.Fullscreen ()
        let exposeEvent = drawingArea.ExposeEvent |> Event.map (fun _ -> DrawingAreaExposed)
        let deleteEvent = this.DeleteEvent |> Event.map (fun e -> DeleteWindow e)
        let startTime = startClock ()
        let startPhotos = startFilePolling ()
        startCecInputLoop ()
        List.reduce Event.merge [exposeEvent; deleteEvent; clockEvent.Publish; photosChangedEvent.Publish; cecEvent.Publish]
        |> Event.scan
            (fun previousEvents newEvent -> newEvent :: previousEvents) [startPhotos; startTime]
        |> Event.add
            (function
            | DeleteWindow e :: _ ->
                Application.Quit ()
                e.RetVal <- true
            | DrawingAreaExposed :: tail ->
//              Serilog.Log.Warning ("display time lag {Milliseconds}", (DateTime.Now - getTime tail).Milliseconds)
                let photos, photosTime, accumulatedLeftAndRightButtons = getPhotos 0 tail
                let pixbuf, markup = display photos accumulatedLeftAndRightButtons photosTime (getTime tail)
                paintDrawingArea drawingArea pixbuf markup
                Thread.Sleep 500
                this.QueueDraw ()
            | Clock _ :: _ ->
                ()
            | e :: _ ->
                Serilog.Log.Information ("{Event:l}", e.toString)
            | [] ->
                ())
