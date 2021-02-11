namespace Mikodev.Mines.Viewer

open Avalonia
open Avalonia.Controls.ApplicationLifetimes
open Avalonia.Markup.Xaml

type App() as me =
    inherit Application()

    do
        AvaloniaXamlLoader.Load me

    override x.OnFrameworkInitializationCompleted() =
        match x.ApplicationLifetime with
        | :? IClassicDesktopStyleApplicationLifetime as desktop ->
             desktop.MainWindow <- new MainWindow()
        | _ -> ()

        base.OnFrameworkInitializationCompleted()
