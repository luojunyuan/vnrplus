module Main

open System.IO
open Avalonia.Controls
open Avalonia.FuncUI
open Avalonia.FuncUI.DSL
open Avalonia.Layout

let view () =
    Component(fun ctx ->
        let state = ctx.useState "nothing new here"

        DockPanel.create [
            DockPanel.children [
                Button.create [
                    Button.dock Dock.Bottom
                    Button.onClick (fun _ ->
                        state.Set("waiting...")
                        let fileWatcher = new FileSystemWatcher()
                        fileWatcher.Path <- "/tmp/"
                        fileWatcher.Filter <- "wine_out"
                        fileWatcher.NotifyFilter <- NotifyFilters.LastWrite
                        fileWatcher.EnableRaisingEvents <- true
                        let handleFileChange _ (e: FileSystemEventArgs) =
                            if e.ChangeType = WatcherChangeTypes.Changed then
                                use fileStream = File.OpenRead e.FullPath
                                use streamReader = new StreamReader(fileStream)
                                let content = streamReader.ReadLine()
                                state.Set(content)

                        fileWatcher.Changed.AddHandler(FileSystemEventHandler(handleFileChange)))
                    Button.content "Read begin"
                    Button.horizontalAlignment HorizontalAlignment.Stretch
                    Button.horizontalContentAlignment HorizontalAlignment.Center
                ]
                Button.create [
                    Button.dock Dock.Bottom
                    Button.onClick (fun _ -> ())
                    Button.content "Stop"
                    Button.horizontalAlignment HorizontalAlignment.Stretch
                    Button.horizontalContentAlignment HorizontalAlignment.Center
                ]
                TextBlock.create [
                    TextBlock.dock Dock.Top
                    TextBlock.fontSize 28.0
                    TextBlock.verticalAlignment VerticalAlignment.Center
                    TextBlock.horizontalAlignment HorizontalAlignment.Center
                    TextBlock.text state.Current
                ]
            ]
        ]
    )