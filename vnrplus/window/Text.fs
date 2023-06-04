module Text

open Elmish
open System.Runtime.InteropServices
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
open FSharp.Control.Reactive
open HookParam
open Microsoft.Win32
open System
open System.Diagnostics
open System.IO

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

let init () =
    printfn "Text init"

    { text = "init"
      threadId = 0
      listenButtonEnabled = true
      kanaEnable = false
      mecabWords = [] }

type Msg =
    | ChangeThreadId of string
    | ReceiveHookParameter of HookParam.HookParameter
    | DisableButton
    | ChangeTextBlock of bool
    | UpdateText of MeCab.MeCabWord list
    | Test

let mutable currentText = ""
let mutable tmp = false
let lineBreaker = Subject.broadcast
let tagger = MeCab.createTagger()

let LineBreakerSubscript (dispatch: Msg -> unit) =
    lineBreaker
    |> Observable.throttle (TimeSpan.FromMilliseconds 50)
    |> Observable.observeOn Avalonia.ReactiveUI.AvaloniaScheduler.Instance
    |> Observable.subscribe (fun () ->
        // currentText.length
        // Start mecab text
        let mecabWords = MeCab.generateWords tagger currentText |> Seq.toList
        dispatch (UpdateText mecabWords)
        dispatch (ChangeTextBlock true)
        tmp <- true) // Should dispatch with message
    |> ignore

let update (msg: Msg) (state: State) : State =
    match msg with
    | ChangeThreadId text ->
        match Int32.TryParse text with
        | true, i -> { state with threadId = i }
        | _ -> state
    | ReceiveHookParameter hp ->

        if (state.threadId = 0) then
            { state with
                text = state.text + " " + hp.text }
        else if (hp.index = state.threadId) then
            lineBreaker.OnNext()
            // if(split[1] = "␍") then printfn "line break"
            if (tmp = true) then
                // new text start
                currentText <- hp.text
                tmp <- false

                { state with
                    text = hp.text
                    kanaEnable = false }
            else
                currentText <- state.text + hp.text

                { state with
                    text = state.text + hp.text }
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
                    PartOfSpeech = MeCab.Hinshi.代名詞 } ]
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
                      TextBlock.padding (0, 14, 0, 0) //  should equal ruby size
                      TextBlock.isVisible (not state.kanaEnable)
                      TextBlock.lineHeight 42 // also effect by ruby size
                      AttrBuilder<TextBlock>.CreateProperty<IEffect>(TextBlock.EffectProperty, blur, ValueNone) 
                    ]

                WrapPanel.create
                    [ WrapPanel.dock Dock.Top
                      WrapPanel.horizontalAlignment HorizontalAlignment.Left
                      WrapPanel.isVisible state.kanaEnable
                      WrapPanel.orientation Orientation.Horizontal
                      WrapPanel.children
                          [ for moji in state.mecabWords do
                                Ruby.Word moji.Kana moji.Word ]
                      AttrBuilder<WrapPanel>.CreateProperty<IEffect>(TextBlock.EffectProperty, blur, ValueNone)
                    ] ] ]

let setBackgroundTheme opacity theme =
    if theme = Styling.ThemeVariant.Light then
        SolidColorBrush(Colors.Black, opacity)
    else SolidColorBrush(Colors.White, opacity)

type TextWindow(hpEvent: HookParamEvent) as this =
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
        // base.TransparencyLevelHint <- [| WindowTransparencyLevel.Transparent |]
        base.TransparencyLevelHint <- WindowTransparencyLevel.Transparent
        if RuntimeInformation.IsOSPlatform(OSPlatform.Windows) then
            SystemEvents.UserPreferenceChanged.Add(fun e -> printfn $"{e.Category}")
            
        this.PointerPressed
        |> Event.filter ( fun e -> e.GetCurrentPoint(this).Properties.IsLeftButtonPressed )
        |> Event.add (fun e -> e |> this.BeginMoveDrag)

        let subscriptions _state =
            let onHookParam (dispatch: Msg->unit) =
                hpEvent.Event.Subscribe(fun a ->
                    (fun _ -> a |> ReceiveHookParameter |> dispatch) |> Dispatcher.UIThread.Invoke
                    printfn $"{a.index} {a.text}")
                
            [ [nameof onHookParam], onHookParam ]
        
        Elmish.Program.mkSimple init update view
        |> Program.withHost this
        |> Program.withSubscription subscriptions
        |> Program.run
