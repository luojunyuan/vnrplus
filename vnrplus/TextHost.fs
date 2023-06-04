// Windows only
module TextHost

open System.Collections.Generic
open System.Runtime.InteropServices
open System.Diagnostics

[<Literal>] 
let TextHostRelativePath = "libs/texthost.dll"

type ProcessCallback = delegate of uint -> unit
type OnCreateThread =
    delegate of int64 * uint * int64 * int64 * int64 *
        [<MarshalAs(UnmanagedType.LPWStr)>] name: string *
        [<MarshalAs(UnmanagedType.LPWStr)>] hcode: string -> unit
type OnRemoveThread = delegate of int64 -> unit
type OnOutputText =
    delegate of int64 * [<MarshalAs(UnmanagedType.LPWStr)>] text : string * uint -> unit


[<DllImport(TextHostRelativePath, CharSet = CharSet.Auto, SetLastError = true)>]
extern bool TextHostInit(ProcessCallback onConnect, ProcessCallback onDisconnect, OnCreateThread onCreateThread, OnRemoveThread onRemoveThread, OnOutputText onOutputText)

[<DllImport(TextHostRelativePath, CharSet = CharSet.Auto, SetLastError = true)>]
extern void InsertHook(uint processId, string hookCode)

[<DllImport(TextHostRelativePath, CharSet = CharSet.Auto, SetLastError = true)>]
extern void RemoveHook(uint processId, int64 address)

[<DllImport(TextHostRelativePath, CharSet = CharSet.Auto, SetLastError = true)>]
extern void InjectProcess(uint processId)

[<DllImport(TextHostRelativePath, CharSet = CharSet.Auto, SetLastError = true)>]
extern void DetachProcess(uint processId)

[<DllImport(TextHostRelativePath, CharSet = CharSet.Auto, SetLastError = true)>]
extern void SearchForText(uint processId, string text, int codepage)

[<DllImport(TextHostRelativePath, CharSet = CharSet.Auto, SetLastError = true)>]
extern void AddClipboardThread(nativeint handle)

[<DllImport(TextHostRelativePath, CharSet = CharSet.Auto, SetLastError = true)>]
extern void UpdateFlushTimeout(uint timeout)


// [Handle:ProcessId:Address :Context :Context2:Name(Engine):HookCode :Text]
// [19    :272C     :769550C0:2C78938 :0       :TextOutA    :HS10@0:gdi32.dll:TextOutA:] ���́c�c�c�c�B
// [2     :2FF0     :75766C70:74CFE309:0       :            :HB0@0:������Ղ�I.exe     :] ActiveMovie WindowActiveMovie Window"
type HookParam =
    { Handle: int64
      ProcessId: uint
      Address: int64
      Context: int64
      Context2: int64
      Name: string
      HookCode: string }

let textThreadDict = Dictionary<int64, HookParam>()
let hptmp = HookParam.HookParamEvent()

let onConnect (processId: uint) : unit = ()
let onDisconnect (processId: uint) : unit = ()
let onCreateThread (threadId: int64) (processId: uint) (address: int64) (context: int64) (subContext: int64) (name: string) (hookcode: string) : unit =
    textThreadDict.Add(
        threadId,
        { Handle = threadId
          ProcessId = processId
          Address = address
          Context = context
          Context2 = subContext
          Name = name
          HookCode = hookcode }
    )
let onRemoveThread (threadId: int64) : unit = ()
let onOutputText (threadId: int64) (text: string) (len: uint) =
    hptmp.TriggerEvent(
        { HookParam.index = int threadId
          HookParam.text = text })

let mutable onConnectDelegate = Unchecked.defaultof<ProcessCallback>
let mutable onDisconnectDelegate = Unchecked.defaultof<ProcessCallback>
let mutable onCreateThreadDelegate = Unchecked.defaultof<OnCreateThread>
let mutable onRemoveThreadDelegate = Unchecked.defaultof<OnRemoveThread>
let mutable onOutputTextDelegate = Unchecked.defaultof<OnOutputText>

let inject (processes: Process array) =
    onConnectDelegate <- onConnect
    onDisconnectDelegate <- onDisconnect
    onCreateThreadDelegate <- onCreateThread
    onRemoveThreadDelegate <- onRemoveThread
    onOutputTextDelegate <- onOutputText
    
    TextHostInit(onConnectDelegate, onDisconnectDelegate, onCreateThreadDelegate, onRemoveThreadDelegate, onOutputTextDelegate) |> ignore
    UpdateFlushTimeout(uint 0)
    for proc in processes do
        InjectProcess(uint proc.Id)
