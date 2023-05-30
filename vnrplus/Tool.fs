module Tool

open System.Diagnostics

let startCxpipe =
    let processInfo = ProcessStartInfo()
    processInfo.FileName <- "/bin/bash"
    processInfo.Arguments <- Common.TmpStartScript
    processInfo.UseShellExecute <- false
    processInfo.RedirectStandardOutput <- true
    processInfo.CreateNoWindow <- true
    
    // Lifetime is binding to game
    let cxpipeWine = Process.Start processInfo
    cxpipeWine.OutputDataReceived.Add(fun e -> printfn $"{e.Data}")
    cxpipeWine.EnableRaisingEvents <- true
    cxpipeWine
