module Impure

open System
open System.Diagnostics
open System.IO
open Avalonia.Controls.ApplicationLifetimes
open Common

let startGameWithCxpipe bottleName (gamePath: string) =
    let gameName = Path.GetFileNameWithoutExtension gamePath
    let processInfo = ProcessStartInfo()
    processInfo.FileName <- "/bin/bash"
    processInfo.Arguments <- runGameScriptPath + " " + bottleName + " " + gamePath + " " + gameName + " " + cxpipeExePath
    processInfo.UseShellExecute <- false
    processInfo.RedirectStandardOutput <- true
    processInfo.CreateNoWindow <- true
    
    // Lifetime is binding to game, just recognize it as game
    let cxpipeWineInBash = Process.Start processInfo
    cxpipeWineInBash.OutputDataReceived.Add(fun e -> printfn $"{e.Data}")
    cxpipeWineInBash.EnableRaisingEvents <- true
    cxpipeWineInBash

let startFswatch hookDataCallback =
    // Ensure pipe exist
    File.Create outPipePath |> dispose
    let mutable offset = 0L
    let dataChangeFiredCallback () =
        // Only recognize the event as a trigger
        use fileStream = File.OpenRead outPipePath

        if (offset >= fileStream.Length) then
            offset <- fileStream.Length
            ()

        lock fileStream (fun _ ->
            fileStream.Position <- offset
            use streamReader = new StreamReader(fileStream)
            let content = streamReader.ReadToEnd()
            content
            |> split "\r\n"
            |> Array.iter (fun hookParam ->
                 hookParam
                 |> split "☭"
                 |> HookParam.convertToHookParameter
                 |> hookDataCallback)

            offset <- streamReader.BaseStream.Position)

    let startInfo = ProcessStartInfo()
    startInfo.FileName <- "fswatch"
    startInfo.Arguments <- outPipePath
    startInfo.RedirectStandardOutput <- true
    startInfo.UseShellExecute <- false
    let fswatch = Process.Start startInfo
    fswatch.OutputDataReceived |> Event.add(fun _ -> dataChangeFiredCallback ())
    fswatch.BeginOutputReadLine()
    fswatch
 
let retrieveMainWindow() =
    match Avalonia.Application.Current.ApplicationLifetime with
    | :? IClassicDesktopStyleApplicationLifetime as desktop -> desktop.MainWindow
    | _ -> failwith "Unable to retrieve main window"

let getBottles() = Directory.GetDirectories $"/Users/{Environment.UserName}/Library/Application Support/CrossOver/Bottles"
