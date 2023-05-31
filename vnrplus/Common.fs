module Common

open System
open System.IO
open Avalonia.Controls.ApplicationLifetimes

let inline dispose (x: IDisposable) = x.Dispose()

let TrayIcon = Path.Combine(AppContext.BaseDirectory, "Assets/TrayIcon.ico")
let RunGameScript = Path.Combine(AppContext.BaseDirectory, "run_game.sh")
let FswatchPath = Path.Combine(AppContext.BaseDirectory, "fswatch")
let CxpipePath = Path.Combine(AppContext.BaseDirectory, "cxpipe.exe")

let PipeFullPath = "/tmp/wine_out"

let Split symbol (text: string) = text.Split symbol

let defaultEmpty = Option.defaultValue ""