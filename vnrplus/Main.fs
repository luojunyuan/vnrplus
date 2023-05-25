module Main

open System
open System.Diagnostics
open System.IO
open System.Threading
open Avalonia.Controls
open Avalonia.FuncUI
open Avalonia.FuncUI.DSL
open Avalonia.Layout
open Avalonia.Media

let view () =
    Component(fun ctx ->
        let state = ctx.useState "nothing new here"
        let max = ctx.useState System.Int32.MaxValue
        let id = ctx.useState 0

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
                        
                        // TODO: wine_out may not exist
                        use fileStream = File.OpenRead fullPath
                        let mutable offset = fileStream.Seek(0, SeekOrigin.End)
                        
                        let fswatch = Process.Start startInfo
                        fswatch.OutputDataReceived.Add(fun _ ->
                            // Only recognize the event as a trigger
                            use fileStream = File.OpenRead fullPath
                            lock fileStream (fun _ ->
                                fileStream.Position <- offset;
                                use streamReader = new StreamReader(fileStream)
                                let content = streamReader.ReadToEnd()
                                
                                let multiParam = content.Split("\r\n", StringSplitOptions.RemoveEmptyEntries)
                                for hookParam in multiParam do
                                    let split = hookParam.Split(' ')
                                    if (split[0] = string id.Current) then
                                        state.Set(state.Current + split[1])
                                offset <- streamReader.BaseStream.Position
                                )
                        )
                        fswatch.BeginOutputReadLine()
                        )
                            
                    Button.content "Read begin"
                    Button.horizontalAlignment HorizontalAlignment.Stretch
                    Button.horizontalContentAlignment HorizontalAlignment.Center
                ]
                Button.create [
                    Button.dock Dock.Bottom
                    Button.onClick (fun _ -> state.Set(id.Current.ToString()))
                    Button.content "Stop"
                    Button.horizontalAlignment HorizontalAlignment.Stretch
                    Button.horizontalContentAlignment HorizontalAlignment.Center
                ]
                TextBox.create [
                    TextBox.dock Dock.Bottom
                    TextBox.text (string id.Current)
                    TextBox.onTextChanged (fun text ->
                        match Int32.TryParse text with
                        | true, i -> id.Set(i)
                        | _ ->())
                ]
                TextBox.create [
                    TextBox.dock Dock.Top
                    TextBox.fontSize 14.0
                    TextBox.verticalAlignment VerticalAlignment.Top
                    TextBox.horizontalAlignment HorizontalAlignment.Left
                    TextBox.text state.Current
                    TextBox.onTextChanged (fun text -> max.Set(text.Length))
                    TextBox.caretIndex max.Current
                    TextBox.textWrapping TextWrapping.Wrap
                ]
            ]
        ]
    )