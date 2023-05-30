module Tool

open System
open System.Diagnostics
open System.IO

let startGameWithCxpipe bottleName gamePath =
    // /Users/username/Library/Application\ Support/CrossOver/Bottles
    let processInfo = ProcessStartInfo()
    processInfo.FileName <- "/bin/bash"
    processInfo.Arguments <- Common.TmpStartScript + " " + bottleName + " " + gamePath
    processInfo.UseShellExecute <- false
    processInfo.RedirectStandardOutput <- true
    processInfo.CreateNoWindow <- true
    
    // Lifetime is binding to game, just recognize it as game
    let cxpipeWine = Process.Start processInfo
    cxpipeWine.OutputDataReceived.Add(fun e -> printfn $"{e.Data}")
    cxpipeWine.EnableRaisingEvents <- true
    cxpipeWine

open Types
let convertToHookParameter (arr: string array) =
    match arr with
    | [| index; text |] ->
        { index = int index; text = text }
    | _ ->
        printfn "Invalid input"
        { index = -1; text = "" }

let startFswatch () =
    let mutable offset = 0L
    let dataChangeFiredCallback () =
        // Only recognize the event as a trigger
        use fileStream = File.OpenRead Common.pipeFullPath

        if (offset > fileStream.Length) then
            offset <- fileStream.Length

        lock fileStream (fun _ ->
            fileStream.Position <- offset
            use streamReader = new StreamReader(fileStream)
            let content = streamReader.ReadToEnd()

            if content <> "" then
                content
                |> fun c -> c.Split("\r\n", StringSplitOptions.RemoveEmptyEntries)
                |> Seq.iter (fun hookParam ->
                     hookParam
                     |> Common.Split [| '☭' |]
                     |> convertToHookParameter
                     |> hookParamEvent.TriggerEvent)

            offset <- streamReader.BaseStream.Position)
        
    // vnrplus may exit accidentally
    Process.GetProcessesByName("fswatch") |> Array.iter (fun p -> p.Kill())

    if (not (Common.FswatchPath |> File.Exists)) then
        failwith "fswatch not found"

    let startInfo = ProcessStartInfo()
    startInfo.FileName <- "fswatch"
    startInfo.Arguments <- Common.pipeFullPath
    startInfo.RedirectStandardOutput <- true
    startInfo.UseShellExecute <- false
    let fswatch = Process.Start startInfo
    fswatch.OutputDataReceived.Add(fun _ -> dataChangeFiredCallback ())
    fswatch.BeginOutputReadLine()
    fswatch
 