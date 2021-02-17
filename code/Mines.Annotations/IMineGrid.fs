namespace Mikodev.Mines.Annotations

[<AllowNullLiteral>]
type IMineGrid =
    abstract member Status : MineGridStatus

    abstract member XMax : int

    abstract member YMax : int

    abstract member FlagCount : int

    abstract member MineCount : int

    abstract member Get : x : int * y : int -> MineData

    abstract member Set : x : int * y : int -> unit

    abstract member Remove : x : int * y : int -> unit

    abstract member RemoveAll : x : int * y : int -> unit
