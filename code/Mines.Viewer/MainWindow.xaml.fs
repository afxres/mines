namespace Mikodev.Mines.Viewer

open Avalonia.Controls
open Avalonia.Markup.Xaml

type MainWindow () as me =
    inherit Window ()

    do
        AvaloniaXamlLoader.Load me
