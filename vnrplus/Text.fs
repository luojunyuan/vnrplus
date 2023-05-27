module Text

open Avalonia.FuncUI.Hosts
open Avalonia.Platform
open Elmish
open Avalonia.FuncUI.Elmish
open System
open System.Diagnostics
open System.IO
open Avalonia.Controls
open Avalonia.FuncUI.DSL
open Avalonia.Layout
open Avalonia.Media
open Avalonia.Threading
open Common
open FSharp.Control.Reactive

let pipeFullPath = "/tmp/wine_out"

type State =
    { text : string
      threadId : int
      listenButtonEnabled : bool }

let init (path: string) =
    { text = path
      threadId = 0
      listenButtonEnabled = true }

type Msg =
| ChangeThreadId of string
| ReceiveHookParameter of string
| DisableButton

let mutable tmp = false
let lineBreaker = Subject.broadcast
lineBreaker
|> Observable.throttle (TimeSpan.FromMilliseconds 400)
|> Observable.subscribe (fun () -> tmp <- true)// Should dispatch with message
|> ignore

let StartListenPipe dispatch =
    if not (File.Exists pipeFullPath) then File.Create pipeFullPath |> dispose
    use fileStream = File.OpenRead pipeFullPath
    let mutable offset = fileStream.Seek(0, SeekOrigin.End)
    let dataReceivedCallback () =
        // Only recognize the event as a trigger
        use fileStream = File.OpenRead pipeFullPath
        lock fileStream (fun _ ->
            fileStream.Position <- offset;
            use streamReader = new StreamReader(fileStream)
            streamReader.ReadToEnd()
            |> fun c -> c.Split("\r\n", StringSplitOptions.RemoveEmptyEntries)
            |> Array.map (fun hookParam -> (fun _ -> dispatch (ReceiveHookParameter hookParam)))
            |> Array.iter Dispatcher.UIThread.Invoke
            offset <- streamReader.BaseStream.Position)
    
    Process.GetProcessesByName("fswatch") |> Array.iter (fun p -> p.Kill())
    let startInfo = ProcessStartInfo()
    startInfo.FileName <- "fswatch"
    startInfo.Arguments <- pipeFullPath
    startInfo.RedirectStandardOutput <- true
    startInfo.UseShellExecute <- false
    let fswatch = Process.Start startInfo
    fswatch.OutputDataReceived.Add(fun _ -> dataReceivedCallback())
    fswatch.BeginOutputReadLine()
    fswatch
    
let update (msg: Msg) (state: State) : State =
    match msg with
    | ChangeThreadId text -> match Int32.TryParse text with
                             | true, i -> { state with threadId = i }
                             | _ -> state
    | ReceiveHookParameter hookParam ->
        let split = hookParam.Split '☭'
        if (split[0] = string state.threadId) then
            lineBreaker.OnNext ()
            if(split[1] = "␍") then printfn "line break"
            if (tmp = true) then
                tmp <- false
                { state with text = split[1] }
            else
                { state with text = state.text + split[1] }
        else
            state
    | DisableButton -> { state with listenButtonEnabled = false }
        
let view (state: State) (dispatch: Msg -> unit)  =    
    DockPanel.create [
        DockPanel.children [
            StackPanel.create [
                StackPanel.dock Dock.Bottom
                StackPanel.orientation Orientation.Horizontal
                StackPanel.children [
                    TextBox.create [
                        TextBox.text (string state.threadId)
                        TextBox.onTextChanged (fun text -> dispatch (ChangeThreadId text))
                    ]
                    Button.create [
                        Button.onClick (fun _ ->
                                        StartListenPipe dispatch |> ignore
                                        dispatch DisableButton)
                        Button.content "Start"
                        Button.horizontalAlignment HorizontalAlignment.Stretch
                        Button.horizontalContentAlignment HorizontalAlignment.Center
                        Button.isEnabled state.listenButtonEnabled
                        Button.height 32
                    ]
                ]
            ]
            TextBlock.create [
                TextBlock.dock Dock.Top
                TextBlock.fontSize 28.0
                TextBlock.verticalAlignment VerticalAlignment.Top
                TextBlock.horizontalAlignment HorizontalAlignment.Left
                TextBlock.text state.text
                TextBlock.textWrapping TextWrapping.Wrap
                TextBlock.padding (0,4,0,0)
            ]
        ]
    ]

type TextWindow(path: string) as this =
    inherit HostWindow()
    
    do
        base.Height <- 200
        base.Width <- 600
        base.Topmost <- true
        base.MinHeight <- 100
        // https://github.com/AvaloniaUI/Avalonia/issues/6786
        base.ExtendClientAreaToDecorationsHint <- true
        base.ExtendClientAreaChromeHints <- ExtendClientAreaChromeHints.NoChrome

        this.PointerPressed.Add(fun e ->
            match e.GetCurrentPoint(null).Properties.IsLeftButtonPressed with
            | true -> this.BeginMoveDrag(e)
            | _ -> ())

        Elmish.Program.mkSimple (fun () -> init path) update view
        |> Program.withHost this
        |> Program.run
        