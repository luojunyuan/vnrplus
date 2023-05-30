module Text

open Avalonia
open Avalonia.FuncUI.Builder
open Avalonia.FuncUI.Hosts
open Avalonia.FuncUI.Library
open Avalonia.Platform
open Avalonia.FuncUI.Elmish
open Avalonia.Controls
open Avalonia.FuncUI.DSL
open Avalonia.Layout
open Avalonia.Media
open Avalonia.Threading
open Elmish
open FSharp.Control.Reactive
open MeCab
open Microsoft.Win32
open System
open System.Diagnostics
open System.IO

let pipeFullPath = "/tmp/wine_out"
let blur = DropShadowEffect()
blur.Color <- Colors.Black
blur.Opacity <- 0.5
blur.BlurRadius <- 4

type State =
    { text: string
      threadId: int
      listenButtonEnabled: bool
      kanaEnable: bool
      mecabWords: MeCab.MeCabWord list }

let init (path: string) =
    printfn "init"

    { text = path
      threadId = 0
      listenButtonEnabled = true
      kanaEnable = false
      mecabWords = [] }

type Msg =
    | ChangeThreadId of string
    | ReceiveHookParameter of string
    | DisableButton
    | ChangeTextBlock of bool
    | UpdateText of MeCab.MeCabWord list
    | Test

let mutable currentText = ""
let mutable tmp = false
let lineBreaker = Subject.broadcast

let LineBreakerSubscript (dispatch: Msg -> unit) =
    lineBreaker
    |> Observable.throttle (TimeSpan.FromMilliseconds 50)
    |> Observable.observeOn Avalonia.ReactiveUI.AvaloniaScheduler.Instance
    |> Observable.subscribe (fun () ->
        // currentText.length
        // Start mecab text
        let mecabWords = MeCab.generateWords currentText |> Seq.toList
        dispatch (UpdateText mecabWords)
        dispatch (ChangeTextBlock true)
        tmp <- true) // Should dispatch with message
    |> ignore

let StartListenPipe (dispatch: Msg -> unit) =
    let mutable offset = 0L

    let dataReceivedCallback () =
        // Only recognize the event as a trigger
        use fileStream = File.OpenRead pipeFullPath

        if (offset > fileStream.Length) then
            offset <- fileStream.Length

        lock fileStream (fun _ ->
            fileStream.Position <- offset
            use streamReader = new StreamReader(fileStream)
            let content = streamReader.ReadToEnd()

            if content <> "" then
                content
                |> fun c -> c.Split("\r\n", StringSplitOptions.RemoveEmptyEntries)
                |> Seq.iter (fun hookParam ->
                    Dispatcher.UIThread.Invoke(fun () -> dispatch (ReceiveHookParameter hookParam)))

            offset <- streamReader.BaseStream.Position)

    Process.GetProcessesByName("fswatch") |> Array.iter (fun p -> p.Kill())

    if (not (File.Exists(Path.Combine(AppContext.BaseDirectory, "fswatch")))) then
        failwith "fswatch not found"

    let startInfo = ProcessStartInfo()
    startInfo.FileName <- "fswatch"
    startInfo.Arguments <- pipeFullPath
    startInfo.RedirectStandardOutput <- true
    startInfo.UseShellExecute <- false
    let fswatch = Process.Start startInfo
    fswatch.OutputDataReceived.Add(fun _ -> dataReceivedCallback ())
    fswatch.BeginOutputReadLine()
    fswatch

let update (msg: Msg) (state: State) : State =
    match msg with
    | ChangeThreadId text ->
        match Int32.TryParse text with
        | true, i -> { state with threadId = i }
        | _ -> state
    | ReceiveHookParameter hookParam ->
        let split = hookParam.Split '☭'
        printfn $"{split[0]}"

        if (state.threadId = 0) then
            { state with
                text = state.text + " " + split[0] }
        else if (split[0] = string state.threadId) then
            lineBreaker.OnNext()
            // if(split[1] = "␍") then printfn "line break"
            if (tmp = true) then
                // new text start
                currentText <- split[1]
                tmp <- false

                { state with
                    text = split[1]
                    kanaEnable = false }
            else
                currentText <- state.text + split[1]

                { state with
                    text = state.text + split[1] }
        else
            state
    | DisableButton ->
        { state with
            listenButtonEnabled = false }
    | ChangeTextBlock s -> { state with kanaEnable = s }
    | UpdateText v -> { state with mecabWords = v }
    | Test ->
        { state with
            mecabWords =
                [ { Word = "草"
                    Kana = "くさ"
                    PartOfSpeech = Hinshi.代名詞 } ]
                @ state.mecabWords }

let view (state: State) (dispatch: Msg -> unit) =
    DockPanel.create
        [ DockPanel.children
              [ StackPanel.create
                    [ StackPanel.dock Dock.Bottom
                      StackPanel.orientation Orientation.Horizontal
                      StackPanel.children
                          [ TextBox.create
                                [ TextBox.text (string state.threadId)
                                  TextBox.onTextChanged (fun text -> text |> ChangeThreadId |> dispatch) ]
                            Button.create
                                [ Button.onClick (fun _ ->
                                      StartListenPipe dispatch |> ignore //cxpipe proc
                                      DisableButton |> dispatch 
                                      LineBreakerSubscript dispatch)
                                  Button.content "Start"
                                  Button.horizontalAlignment HorizontalAlignment.Stretch
                                  Button.horizontalContentAlignment HorizontalAlignment.Center
                                  Button.isEnabled state.listenButtonEnabled
                                  Button.height 32 ]
                            Button.create
                                [ Button.onClick (fun _ -> Test |> dispatch)
                                  Button.content "Test"
                                  Button.horizontalAlignment HorizontalAlignment.Stretch
                                  Button.horizontalContentAlignment HorizontalAlignment.Center
                                  Button.height 32 ] ] ]

                TextBlock.create
                    [ TextBlock.dock Dock.Top
                      TextBlock.fontSize 28.0
                      TextBlock.verticalAlignment VerticalAlignment.Top
                      TextBlock.horizontalAlignment HorizontalAlignment.Left
                      TextBlock.text state.text
                      TextBlock.textWrapping TextWrapping.Wrap
                      TextBlock.padding (0, 13, 0, 0) //  should equal ruby size
                      TextBlock.isVisible (not state.kanaEnable)
                      TextBlock.lineHeight 41 // also effect by ruby size
                      AttrBuilder<TextBlock>
                          .CreateProperty<IEffect>(TextBlock.EffectProperty, blur, ValueNone) ]

                WrapPanel.create
                    [ WrapPanel.dock Dock.Top
                      WrapPanel.horizontalAlignment HorizontalAlignment.Left
                      WrapPanel.isVisible state.kanaEnable
                      WrapPanel.orientation Orientation.Horizontal
                      WrapPanel.children
                          [ for moji in state.mecabWords do
                                Ruby.Word moji.Kana moji.Word ]
                      AttrBuilder<WrapPanel>
                          .CreateProperty<IEffect>(TextBlock.EffectProperty, blur, ValueNone) ] ] ]

let setBackgroundTheme opacity theme =
    if theme = Styling.ThemeVariant.Light then
        SolidColorBrush(Colors.Black, opacity)
    else
        SolidColorBrush(Colors.White, opacity)

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
        base.Padding <- Thickness(0, 4, 0, 0)
        base.Background <- setBackgroundTheme 0.4 Styling.ThemeVariant.Default
        base.TransparencyLevelHint <- WindowTransparencyLevel.Transparent
#if WINDOWS
        SystemEvents.UserPreferenceChanged.Add(fun e -> printfn $"{e.Category}")
#endif

        this.PointerPressed.Add(fun e ->
            if e.GetCurrentPoint(this).Properties.IsLeftButtonPressed then
                e |> this.BeginMoveDrag)

        Elmish.Program.mkSimple (fun () -> init path) update view
        |> Program.withHost this
        |> Program.run
