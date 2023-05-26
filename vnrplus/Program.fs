module Program

open Avalonia
open Avalonia.Controls
open Avalonia.Controls.ApplicationLifetimes
open Avalonia.Themes.Fluent
open Avalonia.FuncUI.Hosts
open Elmish
open Avalonia.FuncUI.Elmish
open Avalonia.FuncUI

type MainWindow() as this =
    inherit HostWindow()
    do
        base.Title <- "Visual Novel Reader Plus"
        base.Width <- 400
        
        this.Closing.Add(fun e ->
            e.Cancel <- true
            this.Hide())
        
        Elmish.Program.mkSimple Main.init Main.update Main.view
        |> Program.withHost this
        |> Program.run

type App() =
    inherit Application()

    override this.Initialize() =
        this.Styles.Add (FluentTheme())
        this.RequestedThemeVariant <- Styling.ThemeVariant.Default

    override this.OnFrameworkInitializationCompleted() =
        match this.ApplicationLifetime with
        | :? IClassicDesktopStyleApplicationLifetime as desktopLifetime ->
            let mainWindow = MainWindow()
            VnrpTrayIcon.TrayIcon mainWindow desktopLifetime
            
            desktopLifetime.ShutdownMode <- ShutdownMode.OnExplicitShutdown
            match desktopLifetime.Args |> Array.toList |> List.tryHead with
            | Some path -> printfn $"{path}"
            | _ -> desktopLifetime.MainWindow <- mainWindow
        | _ -> ()

[<EntryPoint>]
let main(args: string[]) =
    AppBuilder
        .Configure<App>()
        .UsePlatformDetect()
        .UseSkia()
        .StartWithClassicDesktopLifetime(args)