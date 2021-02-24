namespace Mikodev.Mines.Viewer

open Avalonia.Controls
open Avalonia.Interactivity
open Avalonia.Markup.Xaml
open Mikodev.Mines.Annotations
open Mikodev.Mines.Elements
open System
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
        let f = me.Find<TextBlock> "marker"
        let i = TimeSpan.FromMilliseconds (double 33)
        let mutable p = -1
        let mutable q = -1
        async {
            while not source.IsCancellationRequested do
                let e = stopwatch.Elapsed
                t.Text <- e.ToString @"d'.'hh':'mm':'ss'.'ff"
                let g = me.DataContext :?> IMineGrid
                if (g <> null) then
                    let a = g.FlagCount
                    let b = g.MineCount
                    if (a <> p || b <> q) then
                        p <- a
                        q <- b
                        f.Text <- $"{p} / {q}"
                do! Async.Sleep i
            ()
        }

    let statusChangedHandler = EventHandler(fun _ _ ->
        let g = me.DataContext :?> IMineGrid
        match g.Status with
        | MineGridStatus.Wait -> stopwatch.Start()
        | MineGridStatus.Over -> stopwatch.Stop(); banner.Text <- "Game Over!"
        | MineGridStatus.Done -> stopwatch.Stop(); banner.Text <- "Nice Done!"
        | _ -> ()
        ())

    let reopen (g : IMineGrid) =
        let o = me.DataContext :?> IMineGrid
        me.DataContext <- g
        g.StatusChanged.AddHandler statusChangedHandler
        if o <> null then
            o.StatusChanged.RemoveHandler statusChangedHandler
        viewer.Child <- MineDisplayControl(me :> TopLevel, g)
        stopwatch.Reset()
        banner.Text <- String.Empty
        ()

    let clickHandler = EventHandler<RoutedEventArgs>(fun _ e ->
        let b = e.Source :?> Button
        match b.Name with
        | "reopen" ->
            let g = me.DataContext :?> IMineGrid
            reopen (MineGrid(g.XMax, g.YMax, g.MineCount) :> IMineGrid)
            ()
        | "change" ->
            let g = me.DataContext :?> IMineGrid
            let w = MineConfigWindow()
            w.DataContext <- g
            let a = async {
                do! w.ShowDialog me |> Async.AwaitTask
                let r = w.DataContext :?> IMineGrid
                if not (obj.ReferenceEquals(g, r)) then
                    reopen r
                ()
            }
            a |> Async.StartImmediate
            ()
        | "remove" -> Laboratory.remove (me.DataContext :?> IMineGrid)
        | "remark" -> Laboratory.remark (me.DataContext :?> IMineGrid)
        | _ -> ()
        me.Renderer.AddDirty viewer.Child
        ())

    let opened () =
        me.AddHandler(Button.ClickEvent, clickHandler)
        ticker () |> Async.StartImmediate
        reopen (MineGrid(30, 16, 99) :> IMineGrid)
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
