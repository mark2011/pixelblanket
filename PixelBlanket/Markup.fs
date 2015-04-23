module PixelBlanket.Markup
open System
open PixelBlanket.Types
open System.Text
let numberOfLines (text:string) =
    (text.Split ([|'\n'|])).Length
let widthForHeight n =
    let basicWidth = aspectWidth n
    basicWidth * 2 + 3
let withSpacer height (line:string) =
    new String (' ', widthForHeight height - line.Length) + line
let lowerRight height text =
    let header = new String ('\n', height)
    sprintf "%s%s" header (withSpacer height text)
let rightList height (lines:string list) =
    let b = new StringBuilder ()
    for line in lines do
        b.AppendLine (withSpacer height line) |> ignore
    for i in 1..height - lines.Length do
        b.AppendLine "" |> ignore
    b.ToString ()
let markup text =
    let fontSize = screenHeight / (numberOfLines text + 2)
    let fontDesc = sprintf "Courier New Bold %dpx" fontSize
    sprintf "<span font_desc=\"%s\" color=\"%s\">%s</span>" fontDesc "white" text
