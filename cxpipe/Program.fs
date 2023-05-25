open System
open System.Diagnostics
open System.IO
open System.Threading

// This program can be only target to net fx and run by wine.
// cxpipe is predict to be a win-x64 program for Mac. cxpipe -> texthost.dll (win-x64)
// net8: 1.FileSystemWatcher do not work. 2.TextHostExport dll import only work on **AOT**
// Ensure the game has started before running
let pipeOutFile = @"z:\tmp\wine_out"

printfn "start"

// 'notepad'
// 'ぜったい征服☆学園結社パニャニャンダー!!'
// 'ぜったい絶頂☆性器の大発明!!　─処女を狙う学園道具多発エロ─'
let filename = match Environment.GetCommandLineArgs () |> Array.skip 1 with
               | [| arg |] -> arg
               | _ -> failwith "Usage: wine cxpipe.exe filename"

let writer =
    try
        File.CreateText pipeOutFile
    with
        | :? IOException as e ->
            // Pipe is occupied by another process
            match Process.GetProcessesByName("cxpipe") with
            | [||] -> failwith e.Message
            | pipeExist -> for pipe in pipeExist do
                               if (pipe.Id <> Process.GetCurrentProcess().Id) then pipe.Kill()
                           File.CreateText pipeOutFile
writer.AutoFlush <- true

let reader = new FileSystemWatcher()
reader.Path <- @"z:\tmp\"
reader.Filter <- "wine_in"
reader.EnableRaisingEvents <- true
let fileChangeCallback  (e: FileSystemEventArgs) =
    use fileStream = File.OpenRead e.Name
    use streamReader = new StreamReader(fileStream)
    let command = streamReader.ReadLine()
    Trrricksters.Execute command
    printfn $"{e.ChangeType} {command}"
    
reader.Changed.Add(fileChangeCallback)
reader.Renamed.Add(fileChangeCallback)
reader.Created.Add(fileChangeCallback)
reader.Deleted.Add(fileChangeCallback)

let onConnect (processId: uint) : unit = ()
let onDisconnect (processId: uint) : unit = ()
let onRemoveThread (threadId: int64) : unit = ()
let onCreateThread = Trrricksters.onCreateThread
let onOutputText (threadId: int64) (text: string) (length: uint) = Trrricksters.onOutputText threadId text length writer
    
TextHostExport.TextHostInit(onConnect, onDisconnect, onCreateThread, onRemoveThread, onOutputText) |> ignore
TextHostExport.UpdateFlushTimeout (uint 0)
let processes = match Process.GetProcessesByName(filename) with
                | [||] -> failwith $"no process {filename} found"
                | arr -> arr

for proc in processes do
    TextHostExport.InjectProcess(uint proc.Id)

processes[0].WaitForExit()
printfn "stop" // only exit with game process not vnrplus
