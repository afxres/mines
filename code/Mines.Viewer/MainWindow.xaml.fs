namespace Mikodev.Mines.Viewer

open Avalonia.Controls
open Avalonia.Markup.Xaml
open Mikodev.Mines.Annotations
open Mikodev.Mines.Elements
open System.ComponentModel

type MainWindow() as me =
    inherit Window()

    let notify () =
        let grid = me.DataContext :?> IMineGrid
        let tag = me.Find<TextBlock> "marker"
        assert(tag <> null)
        tag.Text <- $"{grid.FlagCount} / {grid.MineCount}"
        ()

    let propertyChangedHandler = PropertyChangedEventHandler(fun _ e ->
        match e.PropertyName with
        | "FlagCount" -> notify ()
        | _ -> ()
        ())

    let update (grid : IMineGrid) =
        let old = me.DataContext
        (grid :?> INotifyPropertyChanged).PropertyChanged.AddHandler propertyChangedHandler
        me.DataContext <- grid
        if old <> null then
            (old :?> INotifyPropertyChanged).PropertyChanged.RemoveHandler propertyChangedHandler
        let box = me.Find<Viewbox> "viewer"
        assert(box <> null)
        box.Child <- MineDisplayControl()
        notify ()
        ()

    do
        AvaloniaXamlLoader.Load me
        update (MineGrid(30, 16, 99))
        ()
