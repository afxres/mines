namespace Mikodev.Mines.Viewer

open Avalonia
open Avalonia.Controls
open Avalonia.Input
open Avalonia.LogicalTree
open Avalonia.Media
open Mikodev.Mines.Abstractions
open System

type MineDisplayControl() as me =
    inherit Control()

    let mutable top : TopLevel = null

    let mutable grid : IMineGrid = null

    let mutable w : int = 0

    let mutable h : int = 0

    let size = 20.0

    let spacing = 2.0

    let radius = 2.0

    let pointerPressedHandler = EventHandler<PointerPressedEventArgs>(fun _ e ->
        top.Renderer.AddDirty me
        ())

    let attached () =
        grid <- me.DataContext :?> IMineGrid
        w <- grid.XMax
        h <- grid.YMax
        me.Width <- double w * size + double (w - 1) * spacing
        me.Height <- double h * size + double (h - 1) * spacing

        top <- me.FindLogicalAncestorOfType<TopLevel>()
        top.PointerPressed.AddHandler pointerPressedHandler
        ()

    let detached () =
        top.PointerPressed.RemoveHandler pointerPressedHandler
        top <- null
        grid <- null
        ()

    let render (d : DrawingContext) =
        for m = 0 to w - 1 do
            for n = 0 to h - 1 do
                let rect = Rect(double m * (size + spacing), double n * (size + spacing), size, size)
                d.DrawRectangle(Brushes.Gray, null, rect, radius, radius)
                ()
        ()

    override __.OnAttachedToLogicalTree e =
        base.OnAttachedToLogicalTree e
        attached ()
        ()

    override __.OnDetachedFromLogicalTree e =
        base.OnDetachedFromLogicalTree e
        detached ()
        ()

    override __.Render d =
        base.Render d
        render d
        ()
