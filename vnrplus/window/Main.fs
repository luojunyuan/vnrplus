module Main

open System.IO
open System.Runtime.InteropServices
open Avalonia.Layout
open Elmish
open Avalonia.Controls
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI.Elmish
open Avalonia.FuncUI.Hosts
open Avalonia.Threading
open Common

type State =
    { isGameRunning: bool
      inUseBottle: string option }

type Msg =
    | Start of string
    | Stop
    | Test

module Commands =
    let startGame bottle game =
        let hpEvent = HookParam.HookParamEvent()
        let game, fswatch =
            match RuntimeInformation.IsOSPlatform(OSPlatform.Windows) with
            | true -> Impure.startGameByPath hpEvent game, None
            | false ->
                let fswatch = Impure.startFswatch(hpEvent.TriggerEvent)
                let game = Impure.startGameWithCxpipe bottle game
                game, Some fswatch
        do Impure.retrieveMainWindow().Hide()
        
        let textWindow =
            match true with
            | true ->
                let win = Some (Text.TextWindow(hpEvent))
                win.Value.Show()
                win
            | false -> None
       
        fun dispatch ->
            async {
                let! _ = Async.AwaitEvent game.Exited
                (fun _ -> Stop |> dispatch) |> Dispatcher.UIThread.Invoke
                match fswatch with
                | Some n -> n.Kill()
                | None -> ()
                match textWindow with
                | Some w -> w.Close()
                | None -> ()
                printfn "GameExited"
            } |> Async.StartImmediate
        |> Cmd.ofEffect
    
let update (msg: Msg) (state: State) : State * Cmd<Msg> =
    match msg with
    | Start path ->
        { state with isGameRunning = true },
        Commands.startGame (state.inUseBottle |> defaultEmpty) path
    | Stop -> { state with isGameRunning = false }, Cmd.none
    | Test ->
        printfn $"{state.isGameRunning}"
        state, Cmd.none

let view (state: State) (dispatch: Msg -> unit) =
    StackPanel.create [
        StackPanel.children [
            TextBlock.create [
                TextBlock.horizontalAlignment HorizontalAlignment.Center
                TextBlock.text ("Game in running: " + string state.isGameRunning)
            ]
            TextBlock.create [
                TextBlock.horizontalAlignment HorizontalAlignment.Center
                TextBlock.text ("Current in using bottle: " + (state.inUseBottle |> defaultEmpty |> wrapWith "\""))
            ]
            WrapPanel.create [
                WrapPanel.children [
                    Button.create [
                        Button.content "Start"
                        Button.onClick (fun _ -> "/Users/kimika/Downloads/seifuku/ぜったい征服☆学園結社パニャニャンダー!!.exe" |> Start |> dispatch) ]
                    Button.create [
                        Button.content "Test"
                        Button.onClick (fun _ -> Test |> dispatch) ]]]]]
    
let init () =
    let bottles = Impure.getBottles()
    
    { isGameRunning = false
      inUseBottle = bottles |> Array.tryHead |> defaultEmpty |> Path.GetFileName |> optionStr }, Cmd.none

type MainWindow(gamePath) as this =
    inherit HostWindow()

    do
        base.Title <- "Visual Novel Reader Plus"
        base.Width <- 400
        // ExtendClientAreaToDecorationsHint with margin (0, 32, 0, 0)

        this.Closing |> Event.add (fun e ->
            e.Cancel <- true
            this.Hide())
        
        let subscriptions _state =
            let whenLoaded dispatch =
                this.Loaded.Subscribe(fun _ ->
                    match gamePath with
                    | Some p -> p |> Start |> dispatch
                    | _ -> ())
                
            [ [nameof whenLoaded], whenLoaded ]
        
        Elmish.Program.mkProgram init update view
        |> Program.withHost this
        |> Program.withSubscription subscriptions
        |> Program.run
