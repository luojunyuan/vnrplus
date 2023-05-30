module Common

open System
open System.IO

let inline dispose (x: IDisposable) = x.Dispose()

let TrayIcon = Path.Combine(AppContext.BaseDirectory, "Assets/TrayIcon.ico")