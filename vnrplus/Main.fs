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

type State = { isGameRunning: bool }

type Msg = | Start

let update (msg: Msg) (state: State) : State =
    match msg with
    | Start ->
        let cxpipeWine = Tool.startCxpipe
        // Side effect
        // cxpipeWine.Exited.Add(fun _ -> true |> GameExited)
        // let textWindow = Text.TextWindow "stasssqssjhbuhbhcfcvughvhuftcutfcuhvhivihrt"
        // cxpipeWine.Exited.Add(fun _ -> Dispatcher.UIThread.Invoke(fun _ -> textWindow.Close()))
        // // TODO close fswatch with textWindow
        // textWindow.Show()
        Common.retrieveMainWindow.Hide()
        { state with isGameRunning = true }

let view (state: State) (dispatch: Msg -> unit) =
    WrapPanel.create
        [ WrapPanel.margin (0, 32, 0, 0)
          WrapPanel.children [ Button.create [ Button.content "Start"; Button.onClick (fun _ -> Start |> dispatch) ] ] ]

let init () = { isGameRunning = false }

type MainWindow() as this =
    inherit HostWindow()

    do
        base.Title <- "Visual Novel Reader Plus"
        base.Width <- 400
        base.ExtendClientAreaToDecorationsHint <- true

        this.Closing.Add(fun e ->
            e.Cancel <- true
            this.Hide())

        Elmish.Program.mkSimple init update view |> Program.withHost this |> Program.run
