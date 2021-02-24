namespace Mikodev.Mines.Annotations

open System

[<AllowNullLiteral>]
type IMineGrid =
    [<CLIEvent>]
    abstract member StatusChanged : IEvent<EventHandler, EventArgs>

    abstract member Status : MineGridStatus

    abstract member XMax : int

    abstract member YMax : int

    abstract member FlagCount : int

    abstract member MineCount : int

    abstract member Get : x : int * y : int -> MineData

    abstract member Set : x : int * y : int * mark : MineMark -> unit

    abstract member Remove : x : int * y : int -> int
