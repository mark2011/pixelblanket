module PixelBlanket.CecClient
open PixelBlanket.Types
open Serilog
open System
open System.Diagnostics
open System.Text
open System.Threading
let cecEvent = new Event<PixelEvent> ()
let mutable lastTvPower = None
let mutable CecSystemProcess:Process = null
let log (message:string) =
    Serilog.Log.Information ("cec input: {Line:l}", message)
let send logMessage (request:string) =
    Serilog.Log.Information ("cec command {Message:l}: {Request:l}", logMessage, request)
    CecSystemProcess.StandardInput.WriteLine request
let parseButton (hex) =
    match hex with
    | "00" -> Select
    | "01" -> (Direction (0, +1))
    | "02" -> (Direction (0, -1))
    | "03" -> (Direction (-1, 0))
    | "04" -> (Direction (+1, 0))
    | "0d" -> Back
    | "20" -> (Digit 0)
    | "21" -> (Digit 1)
    | "22" -> (Digit 2)
    | "23" -> (Digit 3)
    | "24" -> (Digit 4)
    | "25" -> (Digit 5)
    | "26" -> (Digit 6)
    | "27" -> (Digit 7)
    | "28" -> (Digit 8)
    | "29" -> (Digit 9)
    | "44" -> Play
    | "45" -> Stop
    | "48" -> Rewind
    | "49" -> FastForward
    | x -> OtherButton x
let getStatus () =
    send "get status" "tx 10:8f"
let TurnTvOn () =
    send "CecSystem.TurnOn" "as"
let TurnTvOff () =
    send "CecSystem.TurnOff" "standby 0"
let startCecInputLoop () =
    CecSystemProcess <- new Process ()
    let info = CecSystemProcess.StartInfo
    info.UseShellExecute <- false
    info.FileName <- "../../run-subprocess.sh"
    info.Arguments <- "../../cec-client"
    info.RedirectStandardError <- true
    info.RedirectStandardInput <- true
    info.RedirectStandardOutput <- true
    CecSystemProcess.Start () |> ignore
    getStatus ()
    let syncContext = SynchronizationContext.CaptureCurrent ()
    let raise = syncContext.RaiseEvent cecEvent
    let parse (line:string) =
        let suffix n = line.Substring (line.Length - n, n)
        let matches prefix =
            line.StartsWith "TRAFFIC" && line.Contains (">> " + prefix)
        let event =
            if matches "0f:36" then
                Some (TvPower Off)
            else if matches "0f:80" then
                let n = Convert.ToInt32 (string (suffix 5).[0])
                Some (TvInput (Hdmi n))
            else if matches "0f:82" then
                Some (TvInput Tv)
            else if matches "01:83" then
                Some (TvPower On)
            else if matches "01:44" then
                Some (RemoteButton (parseButton (suffix 2)))
            else if matches "01:90" then
                match suffix 2 with
                | "00" -> Some (TvPower On)
                | "01" -> Some (TvPower Off)
                | _ -> None
            else
                None
        match event with
        | None -> ()
        | Some (TvPower power) ->
            if lastTvPower <> Some (TvPower power) then
                lastTvPower <- Some (TvPower power)
                TvPower power |> raise
        | Some event -> event |> raise
    async
        {
        let lineBuilder = new StringBuilder ()
        while true do
            let! bytes = CecSystemProcess.StandardOutput.BaseStream.AsyncRead 40
            for b in bytes do
                if b = 10uy then
                    lineBuilder.ToString () |> parse
                    lineBuilder.Clear () |> ignore
                else
                    lineBuilder.Append (string (char b)) |> ignore
        } |> Async.Start
