module Common

open System
open System.Diagnostics.Contracts
open System.IO

let inline dispose (x: IDisposable) = x.Dispose()
let split (symbol: string) (text: string) = text.Split(symbol, StringSplitOptions.RemoveEmptyEntries)
let defaultEmpty = Option.defaultValue ""
let optionStr csharpStr = if String.IsNullOrEmpty csharpStr then None else Some csharpStr
let wrapWith symbol text: string = symbol + text + symbol 

let outPipePath = "/tmp/wine_out"
let trayIconPath = Path.Combine(AppContext.BaseDirectory, "Assets/TrayIcon.ico")
let runGameScriptPath = Path.Combine(AppContext.BaseDirectory, "run_game.sh")
let fswatchToolPath = Path.Combine(AppContext.BaseDirectory, "fswatch")
let cxpipeExePath = Path.Combine(AppContext.BaseDirectory, "cxpipe.exe")
let unidicDir = Path.Combine(AppContext.BaseDirectory, "UniDic/")
let usrdicPath = Path.GetRelativePath(unidicDir, "path/to/user_dic")

// Process.Start would dami if the file not exist. So I check it explicitly
if (not (fswatchToolPath |> File.Exists)) then
    failwith "fswatch not found"
    