module Main

open System
open System.Diagnostics
open Avalonia.Controls
open Avalonia.Controls.ApplicationLifetimes
open Avalonia.FuncUI.DSL
open Avalonia.Threading

// Adjust flush time 50 - 400
// Skip too long text may not fit for immediate fresh

type State =
    { text : string }

let init () =
    { text = "nothing new here" }

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
                Button.onClick (fun _ -> dispatch Start)
            ]
        ]
    ]
