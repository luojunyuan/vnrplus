open System.Diagnostics
open System.IO
open System.Threading

// This program should only compiled to win-x64 **AOT** and run by wine.
// Ensure the game has started before running
// ep. `wine cxpipe.exe filename`.
printfn "start"

// "ぜったい征服☆学園結社パニャニャンダー!!"
let filename = "ぜったい絶頂☆性器の大発明!!　─処女を狙う学園道具多発エロ─"

let writer =
    try
        File.CreateText "/tmp/wine_out" // automatic map to 'Z:\tmp\wine_out' in wine
    with
        | :? IOException -> failwith "Pipe is occupied by another process"
writer.AutoFlush <- true
    
let onConnect (processId: uint) : unit = ()
let onDisconnect (processId: uint) : unit = ()
let onCreateThread (param1: int64) (param2: uint) (param3: int64) (param4: int64) (param5: int64) (param6: string) (param7: string) : unit = ()
let onRemoveThread (threadId: int64) : unit = ()
let onOutputText (param1: int64) (text: string) (param3: uint) =
    printfn $"{text} {Thread.CurrentThread.ManagedThreadId}"
    writer.BaseStream.Position <- 0 
    writer.WriteLine(text)
    
let proc = Process.GetProcessesByName(filename)[0]
printfn $"{proc.Id}"

TextHostExport.TextHostInit(onConnect, onDisconnect, onCreateThread, onRemoveThread, onOutputText) |> ignore
TextHostExport.InjectProcess(uint proc.Id) |> ignore


System.Console.ReadKey() |> ignore
printfn "stop" // only exit with game process not vnrplus
