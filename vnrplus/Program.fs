module Program

open Avalonia
open Avalonia.Controls
open Avalonia.Controls.ApplicationLifetimes
open Avalonia.Themes.Fluent

type App() =
    inherit Application()

    override this.Initialize() =
        this.Styles.Add(FluentTheme())
        this.RequestedThemeVariant <- Styling.ThemeVariant.Default

    override this.OnFrameworkInitializationCompleted() =
        match this.ApplicationLifetime with
        | :? IClassicDesktopStyleApplicationLifetime as desktopLifetime ->
            let mainWindow = Main.MainWindow()
            VnrpTrayIcon.startTrayIcon mainWindow desktopLifetime

            desktopLifetime.ShutdownMode <- ShutdownMode.OnExplicitShutdown

            desktopLifetime.MainWindow <-
                match desktopLifetime.Args |> Array.tryHead with
                | Some path -> Text.TextWindow path :> Window
                | _ -> mainWindow
        | _ -> ()

[<EntryPoint>]
let main (args: string[]) =
    AppBuilder
        .Configure<App>()
        .UsePlatformDetect()
        .UseSkia()
        .StartWithClassicDesktopLifetime(args)
