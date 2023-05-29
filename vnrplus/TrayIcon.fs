module VnrpTrayIcon

open System
open Avalonia.Controls
open Avalonia.Controls.ApplicationLifetimes
open System.IO

let TrayIcon (mainWindow: Window) (desktopLifetime: IClassicDesktopStyleApplicationLifetime) = 
    let trayContextMenu = NativeMenu();
    let mainMenuItem = NativeMenuItem()
    mainMenuItem.Header <- "Main"
    mainMenuItem.Click.Add (fun _ ->
        mainWindow.Show()
        mainWindow.Activate())
    trayContextMenu.Items.Add mainMenuItem
    let exitMenuItem = NativeMenuItem()
    exitMenuItem.Header <- "Exit"
    exitMenuItem.Click.Add (fun _ -> desktopLifetime.Shutdown())
    trayContextMenu.Items.Add exitMenuItem

    let trayIcon = new TrayIcon()
    trayIcon.Icon <- WindowIcon(Path.Combine(AppContext.BaseDirectory, "Assets/TrayIcon.ico"))
    trayIcon.Menu <- trayContextMenu
    trayIcon.IsVisible <- true
    desktopLifetime.Exit.Add(fun _ ->
        trayIcon.IsVisible <- false
        trayIcon.Dispose())
