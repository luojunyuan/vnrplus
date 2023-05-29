module Ruby

open Avalonia.Controls
open Avalonia.FuncUI.DSL
open Avalonia.Layout
open Avalonia.Media

let Word ruby text =
    StackPanel.create [
        StackPanel.children [
            TextBlock.create [
                TextBlock.text ruby
                TextBlock.fontSize 14
                TextBlock.horizontalAlignment HorizontalAlignment.Center]
            Border.create [
                Border.background (ImageBrush())
                Border.child (TextBlock.create [
                    TextBlock.text text
                    TextBlock.fontSize 28
                    TextBlock.horizontalAlignment HorizontalAlignment.Center ])]
        ]
    ]
