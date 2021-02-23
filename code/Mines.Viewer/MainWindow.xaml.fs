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

    let banner = me.Find<TextBlock> "banner"

    let viewer = me.Find<Viewbox> "viewer"

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
        viewer.Child <- MineDisplayControl(me :> TopLevel, g)
        marker ()
        ()

    let reopen () =
        update (MineGrid(30, 16, 99))
        stopwatch.Reset()
        banner.Text <- String.Empty
        ()

    let clickHandler = EventHandler<RoutedEventArgs>(fun _ e ->
        let b = e.Source :?> Button
        match b.Name with
        | "reopen" -> reopen ()
        | "remove" -> Laboratory.autoRemove (me.DataContext :?> IMineGrid)
        | "remark" -> Laboratory.autoRemark (me.DataContext :?> IMineGrid)
        | _ -> ()
        me.Renderer.AddDirty viewer.Child
        ())

    let opened () =
        me.AddHandler(Button.ClickEvent, clickHandler)
        ticker () |> Async.StartImmediate
        reopen ()
        ()

    let closed () =
        source.Cancel()
        source.Dispose()
        me.RemoveHandler(Button.ClickEvent, clickHandler)
        ()

    override __.OnOpened e =
        base.OnOpened e
        opened ()
        ()

    override __.OnClosed e =
        base.OnClosed e
        closed ()
        ()
