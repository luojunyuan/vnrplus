module Main

open System.Diagnostics
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
                        let fullPath = "/tmp/wine_out"
                        state.Set("waiting...")
                        let startInfo = new ProcessStartInfo()
                        startInfo.FileName <- "fswatch"
                        startInfo.Arguments <- fullPath
                        startInfo.RedirectStandardOutput <- true
                        startInfo.UseShellExecute <- false
                        
                        let fswatch = Process.Start startInfo
                        fswatch.OutputDataReceived.Add(fun (e:DataReceivedEventArgs) ->
                                use fileStream = File.OpenRead fullPath
                                use streamReader = new StreamReader(fileStream)
                                let content = streamReader.ReadLine()
                                let sentence = e.Data + content
                                state.Set(state.Current + "\n" + sentence)
                        )
                        fswatch.BeginOutputReadLine()
                        )
                            
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
                    TextBlock.fontSize 12.0
                    TextBlock.verticalAlignment VerticalAlignment.Center
                    TextBlock.horizontalAlignment HorizontalAlignment.Center
                    TextBlock.text state.Current
                ]
            ]
        ]
    )