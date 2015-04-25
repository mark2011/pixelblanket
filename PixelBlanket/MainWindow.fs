module PixelBlanket.MainWindow
open Gtk
open PixelBlanket.CecClient
open PixelBlanket.Clock
open PixelBlanket.Files
open PixelBlanket.GtkSupport
open PixelBlanket.Keyboard
open PixelBlanket.Markup
open PixelBlanket.Sound
open PixelBlanket.State
open PixelBlanket.Types
open Serilog
open System
open System.Text
open System.Threading
let outputAt (t1:DateTime) = function
    | NoPhotosScreen ->
        None, blackPixbuf, "\n\n\n\n\nPlace 1920x1080 jpg files\n   in folder /home/pi/Pictures\n\n\n" |> markup
    | ClockScreen ->
        let clockText =
            let suffix =
                match t1.Day with
                | 1 | 21 | 31 -> "st"
                | 2 | 22 -> "nd"
                | 3 | 23 -> "rd"
                | _ -> "th"
            String.Format ("Today is {0:dddd}\n{0:MMMMM d}{1}\n{0:yyyy}\n\nThe time is {0:h:mm tt}", t1, suffix)
        Some t1, blackPixbuf, clockText |> markup
    | PhotoScreen photo ->
        let imagePixbuf =
            photo |> getImagePixbuf
        let photoMarkup =
            (if t1.Second < 10 then lowerRight 10 (t1.ToString "h:mm tt") else "") |> markup
        None, imagePixbuf, photoMarkup
type MyWindow () as this = 
    inherit Window ("MainWindow")
    let drawingArea = new DrawingArea ()
    do
        let log = (((new LoggerConfiguration ())
            .WriteTo.ColoredConsole ())
            .WriteTo.RollingFile "/home/pi/pixelblanket-log-{date}.txt").CreateLogger ()
        Serilog.Log.Logger <- log
        screenWidth <- this.Screen.Width
        screenHeight <- this.Screen.Height
        this.ModifyBg (StateType.Normal, Gdk.Color.Zero)
        this.Add drawingArea
        this.ShowAll ()
        this.Fullscreen ()
        let exposeEvent = drawingArea.ExposeEvent |> Event.map (fun _ -> DrawingAreaExposed)
        let deleteEvent = this.DeleteEvent |> Event.map (fun e -> DeleteWindow e)
        let startTime = startClock ()
        let startPhotos = startFilePolling ()
        startKeyboard this
        startCecInputLoop ()
        List.reduce Event.merge [exposeEvent; deleteEvent; keyboardEvent.Publish; clockEvent.Publish; photosChangedEvent.Publish; cecEvent.Publish]
        |> Event.scan
            (fun previousEvents newEvent -> newEvent :: previousEvents) [startPhotos; startTime]
        |> Event.add
            (function
            | DeleteWindow e :: _ ->
                Application.Quit ()
                e.RetVal <- true
            | PixelQuit :: _ ->
                Application.Quit ()
            | DrawingAreaExposed :: tail ->
                let deltaT = (DateTime.Now - getTime tail).Milliseconds
                if deltaT >= 950 then
                    Serilog.Log.Warning ("display time lag {Milliseconds}", deltaT)
                let someChimes, pixbuf, markup = getScreen tail |> outputAt (getTime tail)
                paintDrawingArea drawingArea pixbuf markup
                Chimes.start someChimes
                Thread.Sleep 500
                this.QueueDraw ()
            | Clock _ :: _ ->
                ()
            | e :: _ ->
                Serilog.Log.Information ("{Event:l}", e.toString)
            | [] ->
                ())
