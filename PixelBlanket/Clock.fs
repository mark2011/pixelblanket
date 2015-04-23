module PixelBlanket.Clock
open PixelBlanket.Types
open System
open System.Threading
let nowToTheSecond () =
    let reference = DateTime.Today
    let now = DateTime.Now
    reference + TimeSpan.FromSeconds (float (int (now - reference).TotalSeconds))
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
