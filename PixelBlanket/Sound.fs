module PixelBlanket.Sound
open PixelBlanket.Clock
open System
open System.Diagnostics
open System.Threading
module Amixer =
    let mutable lastDeviceNumber = None
    let setDeviceNumber n =
        if lastDeviceNumber <> Some n then
            lastDeviceNumber <- Some n
            let p = new Process ()
            let info = p.StartInfo
            info.UseShellExecute <- false
            info.FileName <- "amixer"
            info.Arguments <- sprintf "-q cset numid=3 %d" n
            p.Start () |> ignore
    let setDeviceAnalog () =
        setDeviceNumber 1
    let setDeviceHdmi () =
        setDeviceNumber 2
    let setDeviceAuto () =
        setDeviceNumber 3
module Chimes =
    let mutable (lastChimeTime:DateTime option) = None
    let transmit (time:DateTime) n =
        if lastChimeTime <> Some time then
           lastChimeTime <- Some time
           let p = new Process ()
           let info = p.StartInfo
           info.UseShellExecute <- false
           info.FileName <- "../../chimes"
           info.Arguments <- n.ToString ()
           p.Start () |> ignore
    let start = function
        | Some (time:DateTime) ->
            let t = toTheMinute time
            match t.Hour, t.Minute with
            | _, m when m <> 0 -> 1
            | 0, _ -> 12
            | h, _  when h > 12 -> h - 12
            | h, _ -> h
            |> transmit t
        | None -> ()
