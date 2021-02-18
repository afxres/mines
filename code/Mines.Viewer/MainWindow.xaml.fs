namespace Mikodev.Mines.Viewer

open Avalonia.Controls
open Avalonia.Markup.Xaml
open Mikodev.Mines.Annotations
open Mikodev.Mines.Elements
open System
open System.ComponentModel
open System.Diagnostics
open System.Threading

type MainWindow() as me =
    inherit Window()

    let stopwatch = Stopwatch()

    let source = new CancellationTokenSource()

    let ticker () =
        let t = me.Find<TextBlock> "ticker"
        assert (t <> null)
        async {
            while not source.IsCancellationRequested do
                let e = stopwatch.Elapsed
                t.Text <- e.ToString()
                let m = if stopwatch.IsRunning then 1000 - e.Milliseconds else 100
                do! Async.Sleep (TimeSpan.FromMilliseconds (double m))
            ()
        }

    let notify () =
        let grid = me.DataContext :?> IMineGrid
        let tag = me.Find<TextBlock> "marker"
        assert (tag <> null)
        tag.Text <- $"{grid.FlagCount} / {grid.MineCount}"
        ()

    let finish () =
        let grid = me.DataContext :?> IMineGrid
        match grid.Status with
        | MineGridStatus.Wait -> stopwatch.Start()
        | MineGridStatus.Over | MineGridStatus.Done -> stopwatch.Stop()
        | _ -> Debug.Fail "What's wrong?"
        ()

    let propertyChangedHandler = PropertyChangedEventHandler(fun _ e ->
        match e.PropertyName with
        | "Status" -> finish ()
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
        assert (box <> null)
        box.Child <- MineDisplayControl()
        notify ()
        ()

    let closed () =
        source.Cancel()
        source.Dispose()
        ()

    do
        AvaloniaXamlLoader.Load me
        ticker () |> Async.StartImmediate
        update (MineGrid(30, 16, 99))
        ()

    override __.OnClosed e =
        base.OnClosed e
        closed ()
        ()
