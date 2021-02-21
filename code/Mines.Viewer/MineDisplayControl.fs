namespace Mikodev.Mines.Viewer

open Avalonia
open Avalonia.Controls
open Avalonia.Input
open Avalonia.Interactivity
open Avalonia.LogicalTree
open Avalonia.Media
open Mikodev.Mines.Annotations
open Mikodev.Mines.Elements
open System

type MineDisplayControl() as me =
    inherit Control()

    let mutable coordinate : (int * int) option = None

    let mutable top : TopLevel = null

    let mutable grid : IMineGrid = null

    let mutable w : int = 0

    let mutable h : int = 0

    let size = 32.0

    let spacing = 2.8

    let radius = 1.4

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
        let update (p : PointerPointProperties) x y =
            coordinate <- Some (x, y)
            if p.IsLeftButtonPressed then
                grid.Remove(x, y) |> ignore
            elif p.IsRightButtonPressed then
                Operations.toggle grid x y
            ()

        let invoke () =
            let current = e.GetCurrentPoint me
            let properties = current.Properties
            let position = current.Position
            handle position (update properties)
            ()

        if obj.ReferenceEquals(me, e.Source) then
            coordinate <- None
            match grid.Status with
            | MineGridStatus.None | MineGridStatus.Wait -> invoke ()
            | _ -> ()
        ())

    let doubleTappedHandler = EventHandler<RoutedEventArgs>(fun _ e ->
        if obj.ReferenceEquals(me, e.Source) then
            match coordinate with
            | Some (x, y) -> grid.RemoveAll(x, y) |> ignore
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

    let format =
        let text n =
            let font = Typeface.Default.FontFamily
            let face = Typeface(font, FontStyle.Normal, FontWeight.Bold)
            FormattedText(Text = n, Typeface = face, FontSize = 22.0, TextAlignment = TextAlignment.Center, Constraint = Size(size, size))

        let seq = seq {
            for i in 1..7 -> string i
            yield "!"
            yield "?"
        }
        seq |> Seq.map (fun x -> x, text x) |> Map

    let colors =
        let seq = seq {
            let key i = $"Mines.Drawing.Color.{i}"
            let get i = Application.Current.Resources.[key i] :?> ISolidColorBrush

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
        let font = Application.Current.Resources.["Mines.Drawing.Color.Font"] :?> IBrush

        let back rect m =
            d.DrawRectangle(colors.[m], null, rect, radius, radius)

        let text (rect : Rect) i =
            d.DrawText(font, rect.TopLeft, format.[i])

        let face rect m =
            match m with
            | MineData.Tile | MineData.``0`` -> ()
            | MineData.Mine | MineData.MineMiss -> mine d rect
            | MineData.Flag | MineData.FlagMiss -> text rect "!"
            | MineData.What | MineData.WhatMiss -> text rect "?"
            | _ -> text rect (string (int m))

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
