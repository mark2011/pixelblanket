module PixelBlanket.Clock
open PixelBlanket.Types
open System
open System.Threading
let toTheMinute time =
    let reference = DateTime.Today
    reference + TimeSpan.FromMinutes (float (int (time - reference).TotalMinutes))
let toTheSecond time =
    let reference = DateTime.Today
    reference + TimeSpan.FromSeconds (float (int (time - reference).TotalSeconds))
let nowToTheSecond () =
    toTheSecond DateTime.Now
let clockEvent = new Event<PixelEvent> ()
let mutable latchedClock = nowToTheSecond ()
let startClock () =
    let syncContext = SynchronizationContext.CaptureCurrent ()
    syncContext.RaiseEvent clockEvent (Clock latchedClock)
    async
        {
        while true do
            do! Async.Sleep 100
            while nowToTheSecond () > latchedClock do
                latchedClock <- latchedClock + TimeSpan.FromSeconds 1.0
                syncContext.RaiseEvent clockEvent (Clock latchedClock)
        } |> Async.Start
    Clock latchedClock
