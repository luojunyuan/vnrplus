// Windows only
module TextHost


open System.Collections.Generic
open System.Diagnostics
open System.Runtime.InteropServices

[<Literal>]
let TextHostRelativePath = "libs/texthost.dll"

type ProcessCallback = delegate of uint -> unit

type OnCreateThread =
    delegate of
        int64 *
        uint *
        int64 *
        int64 *
        int64 *
        [<MarshalAs(UnmanagedType.LPWStr)>] name: string *
        [<MarshalAs(UnmanagedType.LPWStr)>] hcode: string ->
            unit

type OnRemoveThread = delegate of int64 -> unit
type OnOutputText = delegate of int64 * [<MarshalAs(UnmanagedType.LPWStr)>] text: string * uint -> unit


[<DllImport(TextHostRelativePath, CharSet = CharSet.Auto, SetLastError = true)>]
extern bool TextHostInit(
    ProcessCallback onConnect,
    ProcessCallback onDisconnect,
    OnCreateThread onCreateThread,
    OnRemoveThread onRemoveThread,
    OnOutputText onOutputText
)

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
// [19    :272C     :769550C0:2C78938 :0       :TextOutA    :HS10@0:gdi32.dll:TextOutA:] 俺は…………。
// [2     :2FF0     :75766C70:74CFE309:0       :            :HB0@0:いちゃぷり！.exe     :] ActiveMovie WindowActiveMovie Window"
type HookParam =
    { Handle: int64
      ProcessId: uint
      Address: int64
      Context: int64
      Context2: int64
      Name: string
      HookCode: string }
    
let onConnect (processId: uint) : unit = ()
let onDisconnect (processId: uint) : unit = ()
let onRemoveThread (threadId: int64) : unit = ()
let textThreadDict = Dictionary<int64, HookParam>()

let onCreateThread
    (threadId: int64)
    (processId: uint)
    (address: int64)
    (context: int64)
    (subContext: int64)
    (name: string)
    (hookcode: string)
    : unit =
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

let mutable private tmpHP:HookParam.HookParamEvent Option = None
let onOutputText (threadId: int64) (text: string) (length: uint) =
    tmpHP.Value.TriggerEvent(
        { HookParam.index = int threadId
          HookParam.text = text })

let inject hpEvent (processes: Process array) =
    tmpHP <- hpEvent
    
    TextHostInit(onConnect, onDisconnect, onCreateThread, onRemoveThread, onOutputText) |> ignore

    UpdateFlushTimeout(uint 0)

    for proc in processes do
        InjectProcess(uint proc.Id)
