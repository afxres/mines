namespace Mikodev.Mines.Viewer

open Avalonia
open Avalonia.Controls
open Avalonia.Input
open Avalonia.Interactivity
open Avalonia.LogicalTree
open Avalonia.Media
open Mikodev.Mines.Abstractions
open System

type MineDisplayControl() as me =
    inherit Control()

    let mutable coordinate : (int * int) option = None

    let mutable top : TopLevel = null

    let mutable grid : IMineGrid = null

    let mutable w : int = 0

    let mutable h : int = 0

    let size = 32.0

    let spacing = 3.0

    let radius = 3.0

    let handle (p : Point) (f : int -> int -> unit) =
        if me.Bounds.Contains p then
            let m = Math.Floor(p.X / (size + spacing)) |> int
            let n = Math.Floor(p.Y / (size + spacing)) |> int
            let rect = Rect(double m * (size + spacing), double n * (size + spacing), size, size)
            if rect.Contains(p) then
                f m n
                top.Renderer.AddDirty me
        ()

    let pointerPressedHandler = EventHandler<PointerPressedEventArgs>(fun _ e ->
        coordinate <- None
        if not grid.IsGameOver then
            let current = e.GetCurrentPoint me
            let properties = current.Properties
            let position = current.Position
            handle position (fun x y ->
                coordinate <- Some (x, y)
                if properties.IsLeftButtonPressed then
                    grid.Remove(x, y)
                elif properties.IsRightButtonPressed then
                    grid.Set(x, y)
                ())
            ())

    let doubleTappedHandler = EventHandler<RoutedEventArgs>(fun _ e ->
        match coordinate with
        | Some (x, y) ->
            grid.RemoveAll(x, y)
        | None -> ()
        ())

    let attached () =
        grid <- me.DataContext :?> IMineGrid
        w <- grid.XMax
        h <- grid.YMax
        me.Width <- double w * size + double (w - 1) * spacing
        me.Height <- double h * size + double (h - 1) * spacing

        top <- me.FindLogicalAncestorOfType<TopLevel>()
        top.DoubleTapped.AddHandler doubleTappedHandler
        top.PointerPressed.AddHandler pointerPressedHandler
        ()

    let detached () =
        top.DoubleTapped.RemoveHandler doubleTappedHandler
        top.PointerPressed.RemoveHandler pointerPressedHandler
        top <- null
        grid <- null
        ()

    let mine =
        let closure (r : DrawingGroup) (d : DrawingContext) (rect : Rect) =
            let c = d.CurrentTransform
            let m = Matrix(c.M11, c.M12, c.M21, c.M22, rect.X, rect.Y)
            let s = d.PushSetTransform m
            r.Draw d
            s.Dispose()
            ()

        let k = "Mines.Drawing.Mine"
        let r = Application.Current.Resources.[k] :?> DrawingGroup
        closure r

    let texts =
        let text n =
            FormattedText(Text = n, Typeface = Typeface.Default, FontSize = 22.0, TextAlignment = TextAlignment.Center, Constraint = Size(size, size))
        let seq = seq {
            for i in 1..7 -> string i
            yield "!"
            yield "?"
        }
        seq |> Seq.map (fun x -> x, text x) |> Map

    let render (d : DrawingContext) =
        let back rect m =
            let brush =
                match m with
                | MineMark.Tile | MineMark.Flag | MineMark.What -> Brushes.Gray
                | MineMark.MineMiss | MineMark.FlagMiss | MineMark.WhatMiss -> Brushes.Red
                | _ -> Brushes.LightGray
            d.DrawRectangle(brush, null, rect, radius, radius)

        let face rect m =
            match m with
            | MineMark.None | MineMark.Tile -> ()
            | MineMark.Mine | MineMark.MineMiss -> mine d rect
            | MineMark.Flag | MineMark.FlagMiss -> d.DrawText(Brushes.White, rect.TopLeft, texts.["!"])
            | MineMark.What | MineMark.WhatMiss -> d.DrawText(Brushes.White, rect.TopLeft, texts.["?"])
            | _ -> d.DrawText(Brushes.Black, rect.TopLeft, texts.[string (int m)])

        for m = 0 to w - 1 do
            for n = 0 to h - 1 do
                let rect = Rect(double m * (size + spacing), double n * (size + spacing), size, size)
                let mark = grid.Get(m, n)
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
