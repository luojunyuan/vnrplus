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
// Skip too long text may not fit for immediate fresh

type State =
    { text : string }

let init () =
    { text = "nothing new here" }

type Msg =
| FillSpace

let update (msg: Msg) (state: State) : State =
    match msg with
    | FillSpace _ -> state
        
let view (state: State) (dispatch: Msg -> unit)  =
    DockPanel.create [
        DockPanel.children [
          
        ]
    ]
