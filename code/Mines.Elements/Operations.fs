module Mikodev.Mines.Elements.Operations

open Mikodev.Mines.Annotations

let get (grid : IMineGrid) x y =
    grid.Get(x, y)

let set (grid : IMineGrid) x y mark =
    grid.Set(x, y, mark)

let remove (grid : IMineGrid) x y =
    grid.Remove(x, y)

let toggle (grid : IMineGrid) x y =
    let i = get grid x y
    match i with
    | MineData.Tile -> set grid x y MineMark.Flag
    | MineData.Flag -> set grid x y MineMark.What
    | MineData.What -> set grid x y MineMark.None
    | _ -> ()
    ()

let reduce (grid : IMineGrid) x y =
    let s = fun struct (_, _, d) -> if d = MineData.Flag then 1 else 0
    let m (g : IMineGrid) = fun struct (m, n) -> struct (m, n, (get g m n))
    let f (g : IMineGrid) =
        fun struct (a, b, d) ->
            if g.Status = MineGridStatus.Wait then
                match d with
                | MineData.Tile -> remove g a b
                | MineData.What -> set g a b MineMark.None; remove g a b
                | _ -> 0
            else
                0

    let n = get grid x y |> int
    if (n > 0 && n < 8) then
        let l = Algorithms.adjacent grid.XMax grid.YMax x y |> Seq.map (m grid) |> Seq.toList
        let s = l |> List.sumBy s
        if (s >= n) then
            l |> List.sumBy (f grid)
        else
            0
    else
        0
