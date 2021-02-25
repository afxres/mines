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

    // 规避后续空引用检查
    do me.DataContext <- MineGrid(2, 2, 2)

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
                let g = me.DataContext |> unbox<IMineGrid>
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
        let g = me.DataContext |> unbox<IMineGrid>
        match g.Status with
        | MineGridStatus.Wait -> stopwatch.Start()
        | MineGridStatus.Over -> stopwatch.Stop(); banner.Text <- "Game Over!"
        | MineGridStatus.Done -> stopwatch.Stop(); banner.Text <- "Nice Done!"
        | _ -> ()
        ())

    let reopen (g : IMineGrid) =
        let o = me.DataContext |> unbox<IMineGrid>
        me.DataContext <- g
        g.StatusChanged.AddHandler statusChangedHandler
        o.StatusChanged.RemoveHandler statusChangedHandler
        viewer.Child <- MineDisplayControl(me :> TopLevel, g)
        stopwatch.Reset()
        banner.Text <- String.Empty
        ()

    let clickHandler = EventHandler<RoutedEventArgs>(fun _ e ->
        let config g =
            let w = MineConfigWindow(DataContext = g)
            async {
                do! w.ShowDialog me |> Async.AwaitTask
                let r = w.DataContext |> unbox<IMineGrid>
                if not (obj.ReferenceEquals(g, r)) then
                    reopen r
            }

        let b = e.Source :?> Button
        let g = me.DataContext |> unbox<IMineGrid>
        match b.Name with
        | "reopen" -> reopen (MineGrid(g.XMax, g.YMax, g.MineCount) :> IMineGrid)
        | "change" -> config g |> Async.StartImmediate
        | "remove" -> Laboratory.remove g
        | "remark" -> Laboratory.remark g
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
