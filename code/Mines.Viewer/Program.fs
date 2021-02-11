module Mikodev.Mines.Viewer.Program

open System
open Avalonia
open Avalonia.Logging

[<CompiledName "BuildAvaloniaApp">]
let buildAvaloniaApp () =
    AppBuilder
        .Configure<App>()
        .UsePlatformDetect()
        .LogToTrace(LogEventLevel.Debug)

[<STAThread>]
[<EntryPoint>]
let main argv =
    buildAvaloniaApp().StartWithClassicDesktopLifetime(argv)
