module TextHostExport

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
