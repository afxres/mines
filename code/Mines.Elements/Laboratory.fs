module Mikodev.Mines.Elements.Laboratory

open Mikodev.Mines.Annotations
open System

let autoRemove (grid : IMineGrid) =
    if grid = null then
        raise (ArgumentNullException(nameof(grid)))
    let w = grid.XMax
    let h = grid.YMax
    let mutable x = 0
    while grid.Status = MineGridStatus.Wait && x < w do
        let mutable y = 0
        while grid.Status = MineGridStatus.Wait && y < h do
            grid.RemoveAll(x, y)
            y <- y + 1
        x <- x + 1
    ()
