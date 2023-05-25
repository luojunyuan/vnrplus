module Trrricksters

open System.Collections.Generic
open System.IO
open System.Threading

// [Handle:ProcessId:Address :Context :Context2:Name(Engine):HookCode :Text]
// [19    :272C     :769550C0:2C78938 :0       :TextOutA    :HS10@0:gdi32.dll:TextOutA:] 俺は…………。
// [2     :2FF0     :75766C70:74CFE309:0       :            :HB0@0:いちゃぷり！.exe     :] ActiveMovie WindowActiveMovie Window"
type HookParam = {
    Handle: int64
    ProcessId: uint
    Address: int64
    Context: int64
    Context2: int64
    Name: string
    HookCode: string
}

type Execute = 
| Insert

type Command = {
    Execute: Execute
    ProcessId: int Option
    HookCode: string Option
}




let Execute (command: string) =
    let splitCommand = command.Split([| ' ' |])
    printfn $"Unknown command: \"{command}\""
   

let textThreadDict = Dictionary<int64, HookParam>()

let onCreateThread (threadId: int64) (processId: uint) (address: int64) (context: int64) (subContext: int64) (name: string) (hookcode: string) : unit =
    textThreadDict.Add(threadId, {
        Handle = threadId
        ProcessId = processId
        Address = address
        Context = context
        Context2 = subContext
        Name = name
        HookCode = hookcode })

let onOutputText (threadId: int64) (text: string) (length: uint) (writer: StreamWriter) =
    // TODO: make sure the content has no any /n/r
    // writer.WriteLine($" {threadId} {length} {text} {textThreadDict[threadId].Name} {textThreadDict[threadId].ProcessId} {textThreadDict[threadId].HookCode}")
    lock writer (fun () -> writer.WriteLine($"{threadId} {text}")) 
    