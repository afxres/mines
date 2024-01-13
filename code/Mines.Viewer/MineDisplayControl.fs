namespace Mikodev.Mines.Viewer

open Avalonia
open Avalonia.Controls
open Avalonia.Controls.Shapes
open Avalonia.Input
open Avalonia.Media
open Mikodev.Mines.Annotations
open Mikodev.Mines.Elements
open System
open System.Globalization

type MineDisplayControl(top : TopLevel, grid : IMineGrid) as me =
    inherit Control()

    let out = struct (-1, -1)

    let mutable up : struct (int * int) = out

    let mutable down : struct (int * int) = out

    let w = grid.XMax

    let h = grid.YMax

    let size = 32.0

    let margin = 2.8

    let radius = 1.4

    let locate (pose : byref<_>) (e : PointerEventArgs) =
        pose <- out
        if obj.ReferenceEquals(me, e.Source) then
            match grid.Status with
            | MineGridStatus.None | MineGridStatus.Wait ->
                let p = e.GetCurrentPoint(me).Position
                if me.Bounds.Contains p then
                    let m = Math.Floor(p.X / (size + margin)) |> int
                    let n = Math.Floor(p.Y / (size + margin)) |> int
                    let rect = Rect(double m * (size + margin), double n * (size + margin), size, size)
                    if rect.Contains(p) then
                        pose <- struct (m, n)
            | _ -> ()
        ()

    let pointerPressedHandler = EventHandler<PointerPressedEventArgs>(fun _ e ->
        locate &down (e :> _)
        ())

    let pointerReleasedHandler = EventHandler<PointerReleasedEventArgs>(fun _ e ->
        locate &up (e :> _)
        if up <> out && up = down then
            let struct (x, y) = up
            let p = e.GetCurrentPoint(me).Properties
            match p.PointerUpdateKind with
            | PointerUpdateKind.LeftButtonReleased -> Operations.remove grid x y |> ignore
            | PointerUpdateKind.RightButtonReleased -> Operations.toggle grid x y
            | _ -> ()
        ())

    let doubleTappedHandler = EventHandler<TappedEventArgs>(fun _ e ->
        if up <> out && up = down then
            let struct (x, y) = up
            Operations.reduce grid x y |> ignore
        ())

    let attached () =
        me.Width <- double w * size + double (w - 1) * margin
        me.Height <- double h * size + double (h - 1) * margin

        top.DoubleTapped.AddHandler doubleTappedHandler
        top.PointerPressed.AddHandler pointerPressedHandler
        top.PointerReleased.AddHandler pointerReleasedHandler
        ()

    let detached () =
        top.DoubleTapped.RemoveHandler doubleTappedHandler
        top.PointerPressed.RemoveHandler pointerPressedHandler
        top.PointerReleased.RemoveHandler pointerReleasedHandler
        ()

    let wrap key =
        let closure (p : Path) (d : DrawingContext) (rect : Rect) =
            let m = Matrix(1, 0, 0, 1, rect.X, rect.Y)
            let s = d.PushTransform m
            p.Render d
            s.Dispose()
            ()

        let item = $"Mines.Drawing.{key}"
        let path = Application.Current.Resources[item] :?> Path
        closure path

    let mine = wrap "Mine"

    let flag = wrap "Flag"

    let what = wrap "What"

    let text =
        let t = Typeface(Typeface.Default.FontFamily, FontStyle.Normal, FontWeight.Bold)
        let b = Application.Current.Resources["Mines.Drawing.Color.Font"] :?> IBrush
        let f n = FormattedText(textToFormat = string n, culture = CultureInfo.CurrentUICulture, flowDirection = FlowDirection.LeftToRight, emSize = 22.0, typeface = t, foreground = b)

        let seq = seq {
            for i = 0 to 8 do
                let v = f i
                let x = (size - v.Width) / 2.0
                let y = (size - v.Height) / 2.0
                let closure (d : DrawingContext) (rect : Rect) =
                    d.DrawText(v, Point(rect.X + x, rect.Y + y))
                yield enum<MineData> i, closure
        }

        let m = seq |> Map
        let invoke (d : DrawingContext) (rect : Rect) e =
            m[e] d rect
        invoke

    let colors =
        let seq = seq {
            let key i = $"Mines.Drawing.Color.{i}"
            let get i = Application.Current.Resources[key i] :?> ISolidColorBrush

            yield MineData.Mine, get "Mine"
            yield MineData.``0``, get "Back"
            for i = 1 to 8 do
                yield (enum<MineData> i), get i
            for i in [ MineData.Tile; MineData.Flag; MineData.What ] do
                yield i, get "Tile"
            for i in [ MineData.MineMiss; MineData.FlagMiss; MineData.WhatMiss ] do
                yield i, get "Miss"
        }
        seq |> Map

    let render (d : DrawingContext) =
        let back rect m =
            d.DrawRectangle(colors[m], null, rect, radius, radius)

        let face rect m =
            match m with
            | MineData.Tile | MineData.``0`` -> ()
            | MineData.Mine | MineData.MineMiss -> mine d rect
            | MineData.Flag | MineData.FlagMiss -> flag d rect
            | MineData.What | MineData.WhatMiss -> what d rect
            | _ -> text d rect m

        for m = 0 to w - 1 do
            for n = 0 to h - 1 do
                let rect = Rect(double m * (size + margin), double n * (size + margin), size, size)
                let mark = Operations.get grid m n
                back rect mark
                face rect mark
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
