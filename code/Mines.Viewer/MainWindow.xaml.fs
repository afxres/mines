namespace Mikodev.Mines.Viewer

open Avalonia.Controls
open Avalonia.Markup.Xaml
open Mikodev.Mines.Annotations
open Mikodev.Mines.Elements
open System
open System.Diagnostics
open System.Threading
open System.Windows.Input

type MainWindow() as me =
    inherit Window()

    do AvaloniaXamlLoader.Load me

    // 规避后续空引用检查
    do me.DataContext <- MineGrid(2, 2, 2)

    let viewer = me.Find<Viewbox> "viewer"

    let banner = me.Find<TextBlock> "banner"

    let asynchronous = me.Find<CheckBox> "asynchronous"

    let stopwatch = Stopwatch()

    let source = new CancellationTokenSource()

    let ticker () =
        let t = me.Find<TextBlock> "ticker"
        let f = me.Find<TextBlock> "marker"
        let i = TimeSpan.FromMilliseconds (double 33)
        let mutable g = me.DataContext |> unbox<IMineGrid>
        let mutable v = g.Version
        async {
            while not source.IsCancellationRequested do
                let e = stopwatch.Elapsed
                t.Text <- e.ToString @"d'.'hh':'mm':'ss'.'ff"
                let t = me.DataContext |> unbox<IMineGrid>
                if not (obj.ReferenceEquals(g, t)) || v <> t.Version then
                    g <- t
                    v <- t.Version
                    f.Text <- $"{t.FlagCount} / {t.MineCount}"
                    me.Renderer.AddDirty viewer.Child
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

    let action tag =
        let config g =
            let w = MineConfigWindow(DataContext = g)
            async {
                do! w.ShowDialog me |> Async.AwaitTask
                let r = w.DataContext |> unbox<IMineGrid>
                if not (obj.ReferenceEquals(g, r)) then
                    reopen r
            }

        let g = me.DataContext |> unbox<IMineGrid>
        let c = asynchronous.IsChecked = Nullable true

        async {
            match tag with
            | "reopen" -> reopen (MineGrid(g.XMax, g.YMax, g.MineCount) :> IMineGrid);
            | "change" -> do! config g
            | "remove" -> do! Laboratory.remove g c
            | "remark" -> do! Laboratory.remark g c
            | "except" -> do! Laboratory.except g c
            | _ -> ()
        }

    let opened () =
        ticker () |> Async.StartImmediate
        reopen (MineGrid(30, 16, 99) :> IMineGrid)
        ()

    let closed () =
        source.Cancel()
        source.Dispose()
        ()

    do
        let mutable enable = true
        let change = Event<_, _>()

        let invoke tag = 
            enable <- false
            change.Trigger(me, EventArgs.Empty)
            Async.StartImmediate(async {
                do! action tag
                enable <- true
                change.Trigger(me, EventArgs.Empty)
            })

        let handle = { 
            new ICommand with
                [<CLIEvent>]
                member __.CanExecuteChanged = change.Publish
                member __.CanExecute _ = enable
                member __.Execute tag = invoke (tag :?> string)
        }

        let center = me.Find<Grid> "center"
        for i in center.Children do
            match i with
            | :? Button as b -> b.Command <- handle
            | _ -> ()
        ()

    override __.OnOpened e =
        base.OnOpened e
        opened ()
        ()

    override __.OnClosed e =
        base.OnClosed e
        closed ()
        ()
