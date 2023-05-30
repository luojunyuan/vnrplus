module VnrpTrayIcon

open Avalonia.Controls
open Avalonia.Controls.ApplicationLifetimes

let startTrayIcon (mainWindow: Window) (desktopLifetime: IClassicDesktopStyleApplicationLifetime) =
    let trayContextMenu = NativeMenu()
    let mainMenuItem = NativeMenuItem()
    mainMenuItem.Header <- "Main"

    mainMenuItem.Click.Add(fun _ ->
        mainWindow.Show()
        mainWindow.Activate())

    trayContextMenu.Items.Add mainMenuItem
    let exitMenuItem = NativeMenuItem()
    exitMenuItem.Header <- "Exit"
    exitMenuItem.Click.Add(fun _ -> desktopLifetime.Shutdown())
    trayContextMenu.Items.Add exitMenuItem

    // Show TrayIcon
    let trayIcon = new TrayIcon()
    trayIcon.Icon <- WindowIcon Common.TrayIcon
    trayIcon.Menu <- trayContextMenu
    trayIcon.IsVisible <- true

    desktopLifetime.Exit.Add(fun _ ->
        trayIcon.IsVisible <- false
        trayIcon.Dispose())
