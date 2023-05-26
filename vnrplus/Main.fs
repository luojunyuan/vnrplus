module Main

open System
open System.Diagnostics
open System.IO
open Avalonia.Controls
open Avalonia.FuncUI.DSL
open Avalonia.Layout
open Avalonia.Media
open Avalonia.Threading
open Common

// Adjust flush time 50 - 400
// Immediate flush or flush after timeout (two status)

// Skip too long text may not fit for immediate fresh
// 文本窗口，可以在下方增加新行，可以设置最大新行，默认1，1-8
// 可以翻页，新文本来了若不在最新页就从新第一行文本处理

let pipeFullPath = "/tmp/wine_out"
type State =
    { text : string
      textIndex : int
      threadId : int }

let init () =
    { text = "nothing new here"
      textIndex = Int32.MaxValue
      threadId = 0 }

type Msg =
| LockTextIndex of int
| ChangeThreadId of string
| ReceiveHookParameter of string
| StartListen of (unit -> unit)

let update (msg: Msg) (state: State) : State =
    match msg with
    | LockTextIndex len -> { state with textIndex = len }
    | ChangeThreadId text -> match Int32.TryParse text with
                             | true, i -> { state with threadId = i }
                             | _ -> state
    | StartListen callback ->
        let startInfo = ProcessStartInfo()
        startInfo.FileName <- "fswatch"
        startInfo.Arguments <- pipeFullPath
        startInfo.RedirectStandardOutput <- true
        startInfo.UseShellExecute <- false
        let fswatch = Process.Start startInfo
        fswatch.OutputDataReceived.Add(fun _ -> callback())
        fswatch.BeginOutputReadLine()
        { state with text = "waiting..." }
    | ReceiveHookParameter hookParam ->
        let split = hookParam.Split '☭'
        if (split[0] = string state.threadId) then
            if(split[1] = "␍") then printfn "line break"
            { state with text = state.text + split[1] }
        else
            state
        
let view (state: State) (dispatch: Msg -> unit)  =
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
            |> Array.toList
            |> List.map (fun hookParam -> (fun _ -> dispatch (ReceiveHookParameter hookParam)))
            |> List.iter Dispatcher.UIThread.Invoke
            offset <- streamReader.BaseStream.Position)
    
    DockPanel.create [
        DockPanel.children [
            Button.create [
                Button.dock Dock.Bottom
                Button.onClick (fun _ -> dispatch (StartListen dataReceivedCallback))
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
            TextBox.create [
                TextBox.dock Dock.Bottom
                TextBox.text (string state.threadId)
                TextBox.onTextChanged (fun text -> dispatch (ChangeThreadId text))
            ]
            TextBox.create [
                TextBox.dock Dock.Top
                TextBox.fontSize 14.0
                TextBox.verticalAlignment VerticalAlignment.Top
                TextBox.horizontalAlignment HorizontalAlignment.Left
                TextBox.text state.text
                TextBox.onTextChanged (fun text -> dispatch (LockTextIndex text.Length))
                TextBox.caretIndex state.textIndex
                TextBox.textWrapping TextWrapping.Wrap
            ]
        ]
    ]
