module Common

open System
open System.IO
open Avalonia.Controls.ApplicationLifetimes

let inline dispose (x: IDisposable) = x.Dispose()

let TrayIcon = Path.Combine(AppContext.BaseDirectory, "Assets/TrayIcon.ico")
let TmpStartScript = Path.Combine(AppContext.BaseDirectory, "tmp_start.sh")
let FswatchPath = Path.Combine(AppContext.BaseDirectory, "fswatch")

let pipeFullPath = "/tmp/wine_out"

let retrieveMainWindow() =
    match Avalonia.Application.Current.ApplicationLifetime with
    | :? IClassicDesktopStyleApplicationLifetime as desktop -> desktop.MainWindow
    | _ -> failwith "Unable to retrieve main window"
    
let Split symbol (text: string) = text.Split symbol
