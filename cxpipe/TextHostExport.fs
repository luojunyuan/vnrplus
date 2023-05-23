module TextHostExport

open System.Runtime.InteropServices

[<Literal>] 
let TextHostRelativePath = "libs/texthost.dll"

type ProcessCallback = delegate of uint -> unit
type OnCreateThread = delegate of int64 * uint * int64 * int64 * int64 * string * string -> unit
type OnRemoveThread = delegate of int64 -> unit
type OnOutputText = delegate of int64 * [<MarshalAs(UnmanagedType.LPWStr)>] text : string * uint -> unit


[<DllImport(TextHostRelativePath, CharSet = CharSet.Auto, SetLastError = true)>]
extern int TextHostInit(ProcessCallback onConnect, ProcessCallback onDisconnect, OnCreateThread onCreateThread, OnRemoveThread onRemoveThread, OnOutputText onOutputText)

[<DllImport(TextHostRelativePath, CharSet = CharSet.Auto, SetLastError = true)>]
extern int InsertHook(uint processId, string hookCode)

[<DllImport(TextHostRelativePath, CharSet = CharSet.Auto, SetLastError = true)>]
extern int RemoveHook(uint processId, int64 address);

[<DllImport(TextHostRelativePath, CharSet = CharSet.Auto, SetLastError = true)>]
extern int InjectProcess(uint processId);

[<DllImport(TextHostRelativePath, CharSet = CharSet.Auto, SetLastError = true)>]
extern int DetachProcess(uint processId);

[<DllImport(TextHostRelativePath, CharSet = CharSet.Auto, SetLastError = true)>]
extern int SearchForText(uint processId, string text, int codepage);

[<DllImport(TextHostRelativePath, CharSet = CharSet.Auto, SetLastError = true)>]
extern int AddClipboardThread(nativeint handle)
