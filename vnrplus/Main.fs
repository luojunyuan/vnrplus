module Main

open Avalonia.Controls
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI.Hosts
open Avalonia.Threading
open Elmish
open Avalonia.FuncUI.Elmish
open Avalonia.FuncUI
open Common

type State =
    { isGameRunning: bool
      inUseBottle: string option }

type Msg =
    | Start
    | Stop
    | Test

let update (msg: Msg) (state: State) : State * Cmd<Msg> =
    match msg with
    | Start ->
        let game = Tool.startGameWithCxpipe (state.inUseBottle |> defaultEmpty) "/Users/kimika/Downloads/seifuku/ぜったい征服☆学園結社パニャニャンダー!!.exe"
        let fswatch = Tool.startFswatch()
        let d = Types.hookParamEvent.Event.Subscribe(fun a -> printfn $"{a.index} {a.text}")
        let whenGameExit dispatch =
            game.Exited.Add(fun _ ->
                (fun _ -> Stop |> dispatch) |> Dispatcher.UIThread.Invoke
                d.Dispose()
                fswatch.Kill()
                printfn "GameExited")
        Tool.retrieveMainWindow().Hide()
        { state with isGameRunning = true }, Cmd.ofEffect whenGameExit
    | Stop -> { state with isGameRunning = false }, Cmd.none
    | Test ->
        printfn $"{state.isGameRunning}"
        state, Cmd.none

let view (state: State) (dispatch: Msg -> unit) =
    WrapPanel.create
        [ WrapPanel.children [
              Button.create [ Button.content "Start"; Button.onClick (fun _ -> Start |> dispatch) ]
              Button.create [ Button.content "Test"; Button.onClick (fun _ -> Test |> dispatch) ]
          ] ]

let init () =
    { isGameRunning = false
      inUseBottle = Some "new" }, Cmd.none

type MainWindow() as this =
    inherit HostWindow()

    do
        base.Title <- "Visual Novel Reader Plus"
        base.Width <- 400
        // ExtendClientAreaToDecorationsHint with margin (0, 32, 0, 0)

        this.Closing.Add(fun e ->
            e.Cancel <- true
            this.Hide())
        
        Elmish.Program.mkProgram init update view
        |> Program.withHost this
        |> Program.run
