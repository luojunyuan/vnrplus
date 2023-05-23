module Main

open Avalonia.Controls
open Avalonia.FuncUI
open Avalonia.FuncUI.DSL
open Avalonia.Layout

let view () =
    Component(fun ctx ->
        let state = ctx.useState 0

        DockPanel.create [
            DockPanel.children [
                Button.create [
                    Button.dock Dock.Bottom
                    Button.onClick (fun _ -> state.Set(state.Current - 1))
                    Button.content "-"
                    Button.horizontalAlignment HorizontalAlignment.Stretch
                    Button.horizontalContentAlignment HorizontalAlignment.Center
                ]
                Button.create [
                    Button.dock Dock.Bottom
                    Button.onClick (fun _ -> state.Set(state.Current + 1))
                    Button.content "+"
                    Button.horizontalAlignment HorizontalAlignment.Stretch
                    Button.horizontalContentAlignment HorizontalAlignment.Center
                ]
                TextBlock.create [
                    TextBlock.dock Dock.Top
                    TextBlock.fontSize 48.0
                    TextBlock.verticalAlignment VerticalAlignment.Center
                    TextBlock.horizontalAlignment HorizontalAlignment.Center
                    TextBlock.text (string state.Current)
                ]
            ]
        ]
    )