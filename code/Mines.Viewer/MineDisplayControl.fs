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

    let size = 20.0

    let spacing = 2.0

    let radius = 2.0

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
        let current = e.GetCurrentPoint me
        let properties = current.Properties
        let position = current.Position

        coordinate <- None
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

    let render (d : DrawingContext) =
        let text n =
            FormattedText(Text = string n, Typeface = Typeface.Default, FontSize = 14.0, TextAlignment = TextAlignment.Center, Constraint = Size(size, size))

        let draw rect (n : obj) =
            let background = if n :? int then Brushes.LightGray else Brushes.Gray
            let foreground = if n :? int then Brushes.DimGray else Brushes.White
            d.DrawRectangle(background, null, rect, radius, radius)
            if string n <> "0" then
                d.DrawText(foreground, rect.TopLeft, text n)
            ()

        for m = 0 to w - 1 do
            for n = 0 to h - 1 do
                let rect = Rect(double m * (size + spacing), double n * (size + spacing), size, size)
                match grid.Get(m, n) with
                | MineMark.Tile -> d.DrawRectangle(Brushes.Gray, null, rect, radius, radius)
                | MineMark.Flag -> draw rect "!"
                | MineMark.What -> draw rect "?"
                | n -> let x = int n in if (uint n) < 8u then draw rect x else ()
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
