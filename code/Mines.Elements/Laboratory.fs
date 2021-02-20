module Mikodev.Mines.Elements.Laboratory

open Mikodev.Mines.Annotations
open System

let autoRemove (grid : IMineGrid) =
    if grid = null then
        raise (ArgumentNullException(nameof(grid)))
    let w = grid.XMax
    let h = grid.YMax

    let remove () =
        let mutable n = 0
        let mutable x = 0
        while grid.Status = MineGridStatus.Wait && x < w do
            let mutable y = 0
            while grid.Status = MineGridStatus.Wait && y < h do
                n <- n + grid.RemoveAll(x, y)
                y <- y + 1
            x <- x + 1
        n

    while remove () <> 0 do ()
    ()

let autoRemark (grid : IMineGrid) =
    if grid = null then
        raise (ArgumentNullException(nameof(grid)))
    let w = grid.XMax
    let h = grid.YMax
    let mutable x = 0
    while grid.Status = MineGridStatus.Wait && x < w do
        let mutable y = 0
        while grid.Status = MineGridStatus.Wait && y < h do
            let m = grid.Get(x, y)
            if int m >= 1 && int m <= 8 then
                let l = Algorithms.mapAdjacentIndexes w h x y (fun a b -> grid.Get(a, b)) |> Seq.toList
                let n = l |> Seq.filter (fun i -> uint i > 8u) |> Seq.length
                if (n = int m) then
                    Algorithms.mapAdjacentIndexes w h x y (fun a b ->
                        if uint (grid.Get(a, b)) > 8u then
                            while grid.Get(a, b) <> MineData.Flag do
                                grid.Set(a, b)
                        ()) |> Seq.iter id
            y <- y + 1
        x <- x + 1
    ()
