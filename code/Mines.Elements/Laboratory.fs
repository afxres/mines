module Mikodev.Mines.Elements.Laboratory

open Mikodev.Mines.Annotations
open System.Collections.Generic

let remove (grid : IMineGrid) =
    let w = grid.XMax
    let h = grid.YMax

    let invoke () =
        let mutable n = 0
        for x = 0 to w - 1 do
            for y = 0 to h - 1 do
                n <- n + Operations.reduce grid x y
        n

    if grid.Status = MineGridStatus.Wait then
        try
            while invoke () <> 0 do ()
        with
        | MineGridStatusException _ -> ()
    ()

let remark (grid : IMineGrid) =
    let w = grid.XMax
    let h = grid.YMax

    let choose x y = seq {
        for struct (a, b) in Algorithms.adjacent w h x y do
            if Operations.get grid a b |> int > 8 then
                yield struct (a, b)
    }

    let invoke () =
        for x = 0 to w - 1 do
            for y = 0 to h - 1 do
                let n = Operations.get grid x y
                if int n >= 1 && int n <= 8 then
                    let l = choose x y |> Seq.toList
                    if (l |> List.length = int n) then
                        for struct (a, b) in l do Operations.set grid a b MineMark.Flag

    if grid.Status = MineGridStatus.Wait then
        try
            invoke ()
        with
        | MineGridStatusException _ -> ()
    ()

let except (grid : IMineGrid) =
    let w = grid.XMax
    let h = grid.YMax

    let flag a = a = MineData.Flag

    let tile a = a = MineData.Tile || a = MineData.What

    let free n = int n >= 1 && int n <= 8

    let get = fun struct (x, y) -> Operations.get grid x y

    let adjacent = fun struct (x, y) -> Algorithms.adjacent w h x y

    let remark a =
        for struct (x, y) in a do
            Operations.set grid x y MineMark.Flag

    let remove a =
        for struct (x, y) in a do
            Operations.remove grid x y |> ignore

    let choose f a = seq {
        for i in adjacent a do
            if get i |> f then
                yield i
    }

    let reachable a = seq {
        for m in adjacent a do
            for n in choose free m do
                if a <> n then
                    yield n
    }

    let select a b =
        // tiles or question marks (untouched)
        let ua = choose tile a |> Set
        let ub = choose tile b |> Set
        // tiles or question marks (intersection)
        let ui = Set.intersect ua ub
        if (not (Set.isEmpty ui)) then
            let ea = Set.difference ua ui
            let eb = Set.difference ub ui
            // flag count
            let fa = choose flag a |> Seq.length
            let fb = choose flag b |> Seq.length
            // mine count (remaining)
            let ra = int (get a) - fa
            let rb = int (get b) - fb
            // mine count in the intersection (minimum)
            let ca = ra - ea.Count
            let cb = rb - eb.Count
            // mine count in the intersection
            let ci = max ca cb
            if (ci > 0) then
                if (ra <> ca) then
                    if ci = ra then
                        remove ea
                    if ci = ca && Set.isEmpty eb then
                        remark ea
                if (rb <> cb) then
                    if ci = rb then
                        remove eb
                    if ci = cb && Set.isEmpty ea then
                        remark eb
        ()

    let handle (c : HashSet<_>) x y =
        let i = struct (x, y)
        let n = get i
        if (free n && choose flag i|> Seq.length < int n) then
            let items = reachable i |> Seq.toList
            for k in items do
                let a = c.Add(struct (i, k))
                let b = c.Add(struct (k, i))
                if (a && b) then
                    select i k

    let invoke () =
        let c = HashSet<_>()
        for x = 0 to w - 1 do
            for y = 0 to h - 1 do
                handle c x y
        for x = w - 1 downto 0 do
            for y = h - 1 downto 0 do
                handle c x y

    if grid.Status = MineGridStatus.Wait then
        try
            invoke ()
        with
        | MineGridStatusException _ -> ()
    ()
