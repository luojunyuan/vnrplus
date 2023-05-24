open System
open System.Diagnostics
open System.IO
open System.Threading

// This program should only compiled to win-x64 **AOT** and run by wine.
// Ensure the game has started before running
// ep. `wine cxpipe.exe filename`.
printfn "start"

// 'ぜったい征服☆学園結社パニャニャンダー!!'
// 'ぜったい絶頂☆性器の大発明!!　─処女を狙う学園道具多発エロ─'
let filename = match System.Environment.GetCommandLineArgs () |> Array.skip 1 with
               | [| arg |] -> arg
               | _ -> failwith "Usage: wine cxpipe.exe filename"

let writer =
    try
        File.CreateText "/tmp/wine_out" // automatic map to 'Z:\tmp\wine_out' in wine
    with
        | :? IOException -> failwith "Pipe is occupied by another process"
writer.AutoFlush <- true

// let reader = new FileSystemWatcher()
// reader.Path <- "/tmp/"
// reader.Filter <- "wine_in"
// // reader.NotifyFilter <- NotifyFilters.LastWrite
// reader.EnableRaisingEvents <- true
// let fileChangeCallback  (e: FileSystemEventArgs) =
//     printfn $"{e.ChangeType}"
//     // if e.ChangeType = WatcherChangeTypes.Changed then
//     // use fileStream = File.OpenRead e.FullPath
//     // use streamReader = new StreamReader(fileStream)
//     // let content = streamReader.ReadLine()
//     // printfn $"{e.ChangeType} {content}"
//     
// reader.Changed.Add(fileChangeCallback)
// reader.Renamed.Add(fileChangeCallback)
// reader.Created.Add(fileChangeCallback)
// reader.Deleted.Add(fileChangeCallback)

let task = async {
    while true do
        System.Threading.Thread.Sleep(2000)
        // use fileStream = File.OpenRead "/tmp/wine_in"
        // use streamReader = new StreamReader(fileStream)
        // let content = streamReader.ReadLine()
        // printfn $"{content}"
        writer.BaseStream.Position <- 0 
        writer.WriteLine($"{Thread.CurrentThread.ManagedThreadId} {DateTime.Now}")
}

Async.Start(task)

let onConnect (processId: uint) : unit = ()
let onDisconnect (processId: uint) : unit = ()
let onCreateThread (param1: int64) (param2: uint) (param3: int64) (param4: int64) (param5: int64) (param6: string) (param7: string) : unit = ()
let onRemoveThread (threadId: int64) : unit = ()
let onOutputText (threadId: int64) (text: string) (length: uint) =
    writer.BaseStream.Position <- 0 
    writer.WriteLine($"{Thread.CurrentThread.ManagedThreadId} {threadId} {length} {text}")
    
//TextHostExport.TextHostInit(onConnect, onDisconnect, onCreateThread, onRemoveThread, onOutputText) |> ignore

let processes = match Process.GetProcessesByName(filename) with
                | [||] -> failwith $"no process {filename} found"
                | atom -> atom

for proc in processes do
    () //TextHostExport.InjectProcess(uint proc.Id) |> ignore

processes[0].WaitForExit()
printfn "stop" // only exit with game process not vnrplus
