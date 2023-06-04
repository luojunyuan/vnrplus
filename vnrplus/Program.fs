module Program

open Avalonia
open Avalonia.Controls
open Avalonia.Controls.ApplicationLifetimes
open Avalonia.Themes.Fluent

type App() =
    inherit Application()

    override this.Initialize() =
        this.Name <- "Visual Novel Reader Plus"
        this.Styles.Add(FluentTheme())
        this.RequestedThemeVariant <- Styling.ThemeVariant.Default

    override this.OnFrameworkInitializationCompleted() =
        match this.ApplicationLifetime with
        | :? IClassicDesktopStyleApplicationLifetime as desktopLifetime ->
            desktopLifetime.ShutdownMode <- ShutdownMode.OnExplicitShutdown
            desktopLifetime.MainWindow <-
                match desktopLifetime.Args |> Array.tryHead with
                | Some path -> Main.MainWindow(Some path)
                | _ -> Main.MainWindow(None)
                
            do VNRP.TrayIcon.start desktopLifetime.MainWindow desktopLifetime
        | _ -> ()

[<EntryPoint>]
let main (args: string[]) =
    AppBuilder
        .Configure<App>()
        .UsePlatformDetect()
        .UseSkia()
        .StartWithClassicDesktopLifetime(args)
