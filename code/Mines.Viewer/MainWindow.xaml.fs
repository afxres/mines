namespace Mikodev.Mines.Viewer

open Avalonia.Controls
open Avalonia.Interactivity
open Avalonia.Markup.Xaml
open Mikodev.Mines.Annotations
open Mikodev.Mines.Elements
open System
open System.ComponentModel
open System.Diagnostics
open System.Threading

type MainWindow() as me =
    inherit Window()

    do AvaloniaXamlLoader.Load me

    let button = me.Find<Button> "reopen"

    let banner = me.Find<TextBlock> "banner"

    let stopwatch = Stopwatch()

    let source = new CancellationTokenSource()

    let ticker () =
        let t = me.Find<TextBlock> "ticker"
        let i = TimeSpan.FromMilliseconds (double 33)
        async {
            while not source.IsCancellationRequested do
                let e = stopwatch.Elapsed
                t.Text <- e.ToString @"d'.'hh':'mm':'ss'.'ff"
                do! Async.Sleep i
            ()
        }

    let marker () =
        let g = me.DataContext :?> IMineGrid
        let t = me.Find<TextBlock> "marker"
        t.Text <- $"{g.FlagCount} / {g.MineCount}"
        ()

    let status () =
        let g = me.DataContext :?> IMineGrid
        match g.Status with
        | MineGridStatus.Wait -> stopwatch.Start()
        | MineGridStatus.Over -> stopwatch.Stop(); banner.Text <- "Game Over!"
        | MineGridStatus.Done -> stopwatch.Stop(); banner.Text <- "Nice Done!"
        | _ -> Debug.Fail "What's wrong?"
        ()

    let propertyChangedHandler = PropertyChangedEventHandler(fun _ e ->
        match e.PropertyName with
        | "Status" -> status ()
        | "FlagCount" -> marker ()
        | _ -> ()
        ())

    let update (g : IMineGrid) =
        let o = me.DataContext
        (g :?> INotifyPropertyChanged).PropertyChanged.AddHandler propertyChangedHandler
        me.DataContext <- g
        if o <> null then
            (o :?> INotifyPropertyChanged).PropertyChanged.RemoveHandler propertyChangedHandler
        let v = me.Find<Viewbox> "viewer"
        v.Child <- MineDisplayControl()
        marker ()
        ()

    let reopen () =
        update (MineGrid(30, 16, 99))
        stopwatch.Reset()
        banner.Text <- String.Empty
        ()

    let clickHandler = EventHandler<RoutedEventArgs>(fun _ _ ->
        reopen ()
        ())

    let opened () =
        button.Click.AddHandler clickHandler
        ticker () |> Async.StartImmediate
        reopen ()
        ()

    let closed () =
        source.Cancel()
        source.Dispose()
        button.Click.RemoveHandler clickHandler
        ()

    override __.OnOpened e =
        base.OnOpened e
        opened ()
        ()

    override __.OnClosed e =
        base.OnClosed e
        closed ()
        ()
