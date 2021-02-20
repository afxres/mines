module Mikodev.Mines.Elements.Operations

open Mikodev.Mines.Annotations
open System

let get (grid : IMineGrid) x y =
    if grid = null then
        raise (ArgumentNullException (nameof grid))
    grid.Get(x, y)

let set (grid : IMineGrid) x y mark =
    if grid = null then
        raise (ArgumentNullException (nameof grid))
    grid.Set(x, y, mark)

let toggle (grid : IMineGrid) x y =
    if grid = null then
        raise (ArgumentNullException (nameof grid))
    let i = get grid x y
    match i with
    | MineData.Tile -> set grid x y MineMark.Flag
    | MineData.Flag -> set grid x y MineMark.What
    | MineData.What -> set grid x y MineMark.None
    | _ -> ()
    ()
