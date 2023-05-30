module Main

open System
open System.Diagnostics
open Avalonia.Controls
open Avalonia.Controls.ApplicationLifetimes
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI.Hosts
open Avalonia.Threading
open Elmish
open Avalonia.FuncUI.Elmish
open Avalonia.FuncUI

type State =
    { text : string }
    
type Msg =
| Start

let update (msg: Msg) (state: State) : State =
    match msg with
    | Start ->
        let processInfo = ProcessStartInfo()
        processInfo.FileName <- "/bin/bash"
        processInfo.Arguments <- System.IO.Path.Combine(AppContext.BaseDirectory, "tmp_start.sh") 
        processInfo.UseShellExecute <- false
        processInfo.RedirectStandardOutput <- true
        processInfo.CreateNoWindow <- true

        let textWindow = Text.TextWindow "stasssqssjhbuhbhcfcvughvhuftcutfcuhvhivihrt"
        let cxpipeWine = Process.Start processInfo
        cxpipeWine.OutputDataReceived.Add(fun e -> printfn $"{e.Data}")
        cxpipeWine.EnableRaisingEvents <- true
        cxpipeWine.Exited.Add(fun _ -> Dispatcher.UIThread.Invoke(fun _-> textWindow.Close()))
        let window = 
            match Avalonia.Application.Current.ApplicationLifetime with
            | :? IClassicDesktopStyleApplicationLifetime as desktop -> desktop.MainWindow
            | _ -> failwith "Unable to retrieve main window"
        window.Hide()
        // TODO close fswatch with textWindow
        textWindow.Show()
        state
        
let view (state: State) (dispatch: Msg -> unit)  =
    WrapPanel.create [
        WrapPanel.margin (0,32,0,0)
        WrapPanel.children [
            Button.create [
                Button.content "Start"
                Button.onClick (fun _ -> Start |> dispatch)
            ]
        ]
    ]

let init () =
    { text = "nothing new here" }

type MainWindow() as this =
    inherit HostWindow()
    do
        base.Title <- "Visual Novel Reader Plus"
        base.Width <- 400
        base.ExtendClientAreaToDecorationsHint <- true
        
        this.Closing.Add(fun e ->
            e.Cancel <- true
            this.Hide())
        
        Elmish.Program.mkSimple init update view
        |> Program.withHost this
        |> Program.run