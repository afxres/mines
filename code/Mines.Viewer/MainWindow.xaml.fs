namespace Mikodev.Mines.Viewer

open Avalonia.Controls
open Avalonia.Markup.Xaml
open Mikodev.Mines.Elements

type MainWindow() as me =
    inherit Window()

    do
        AvaloniaXamlLoader.Load me
        me.DataContext <- MineGrid(30, 16, 99)
        let box = Viewbox()
        box.Child <- MineDisplayControl()
        me.Content <- box
