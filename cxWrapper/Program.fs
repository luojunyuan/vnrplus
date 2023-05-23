open System.IO

// This program should only compiled to win-x64 AOT and run by wine.
// ep. `wine cxWrapper.exe pid`
printfn "start"

let writer =
    try
        File.CreateText "/tmp/wine_out" // automatic map to 'Z:\tmp\wine_out' in wine
    with
        | :? IOException -> failwith "Pipe is occupead by another process"; 
    
let onConnect (processId: uint) : unit = ()
let onDisconnect (processId: uint) : unit = ()
let onCreateThread (param1: int64) (param2: uint) (param3: int64) (param4: int64) (param5: int64) (param6: string) (param7: string) : unit = ()
let onRemoveThread (threadId: int64) : unit = ()
let onOutputText (param1: int64) (text: string) (param3: uint) =
    printfn $"{text}"
    async {
        do! writer.WriteAsync(text) |> Async.AwaitTask
    } |> Async.RunSynchronously
    
let proc = System.Diagnostics.Process.GetProcessesByName("ぜったい絶頂☆性器の大発明!!　─処女を狙う学園道具多発エロ─")[0]
//let proc = System.Diagnostics.Process.GetProcessesByName("ぜったい征服☆学園結社パニャニャンダー!!")[0]
printfn $"{proc.Id}"

TextHostExport.TextHostInit(onConnect, onDisconnect, onCreateThread, onRemoveThread, onOutputText) |> ignore
TextHostExport.InjectProcess(uint proc.Id) |> ignore

printfn "over" // exit with game process
System.Console.ReadKey() |> ignore;