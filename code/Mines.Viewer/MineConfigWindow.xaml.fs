namespace Mikodev.Mines.Viewer

open Avalonia.Controls
open Avalonia.Interactivity
open Avalonia.Markup.Xaml
open Mikodev.Mines.Annotations
open Mikodev.Mines.Elements
open System

type MineConfigWindow() as me =
    inherit Window()

    do AvaloniaXamlLoader.Load me

    let x = me.Find<TextBox> "x"

    let y = me.Find<TextBox> "y"

    let count = me.Find<TextBox> "count"

    let clickHandler = EventHandler<RoutedEventArgs>(fun _ e ->
        let b = e.Source :?> Button
        let get (t : TextBox) min max back =
            match Int32.TryParse t.Text with
            | true, n ->
                if (n < min) then min
                elif n > max then max
                else n
            | _ -> back

        match b.Name with
        | "accept" ->
            let g = me.DataContext |> unbox<IMineGrid>
            let a = get x 2 600 g.XMax
            let b = get y 2 320 g.YMax
            let c = get count 1 (a * b - 1) g.MineCount
            me.DataContext <- MineGrid(a, b, c)
            ()
        | _ -> ()
        me.Close()
        ())

    let opened () =
        me.AddHandler(Button.ClickEvent, clickHandler)
        let g = me.DataContext |> unbox<IMineGrid>
        x.Text <- string g.XMax
        y.Text <- string g.YMax
        count.Text <- string g.MineCount
        ()

    let closed () =
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
