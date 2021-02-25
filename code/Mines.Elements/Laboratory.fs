﻿module Mikodev.Mines.Elements.Laboratory

open Mikodev.Mines.Annotations

let remove (grid : IMineGrid) =
    let w = grid.XMax
    let h = grid.YMax

    let invoke () =
        let mutable n = 0
        let mutable x = 0
        while grid.Status = MineGridStatus.Wait && x < w do
            let mutable y = 0
            while grid.Status = MineGridStatus.Wait && y < h do
                n <- n + Operations.reduce grid x y
                y <- y + 1
            x <- x + 1
        n

    while invoke () <> 0 do ()
    ()

let remark (grid : IMineGrid) =
    let f g =
        fun struct (a, b) ->
            if Operations.get g a b |> int > 8 then
                Some struct (a, b)
            else
                None

    let w = grid.XMax
    let h = grid.YMax
    let mutable x = 0
    while grid.Status = MineGridStatus.Wait && x < w do
        let mutable y = 0
        while grid.Status = MineGridStatus.Wait && y < h do
            let m = Operations.get grid x y
            if int m >= 1 && int m <= 8 then
                let l = Algorithms.adjacent w h x y |> Seq.choose (f grid) |> Seq.toList
                if (l |> List.length = int m) then
                    for struct (a, b) in l do Operations.set grid a b MineMark.Flag
            y <- y + 1
        x <- x + 1
    ()
