module Mikodev.Mines.Elements.Laboratory

open Mikodev.Mines.Annotations

let remove (grid : IMineGrid) =
    let invoke () =
        let w = grid.XMax
        let h = grid.YMax
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
    let f g =
        fun struct (a, b) ->
            if Operations.get g a b |> int > 8 then
                Some struct (a, b)
            else
                None

    let invoke () =
        let w = grid.XMax
        let h = grid.YMax
        for x = 0 to w - 1 do
            for y = 0 to h - 1 do
                let m = Operations.get grid x y
                if int m >= 1 && int m <= 8 then
                    let l = Algorithms.adjacent w h x y |> Seq.choose (f grid) |> Seq.toList
                    if (l |> List.length = int m) then
                        for struct (a, b) in l do Operations.set grid a b MineMark.Flag

    if grid.Status = MineGridStatus.Wait then
        try
            invoke ()
        with
        | MineGridStatusException _ -> ()
    ()

let intersect (grid : IMineGrid) =
    let w = grid.XMax
    let h = grid.YMax

    let flag = (=) MineData.Flag

    let tile = int >> (<) 8

    let free a = int a > 0 && int a < 8

    let get = fun struct (x, y) -> Operations.get grid x y

    let toggle = fun struct (x, y) -> Operations.set grid x y MineMark.Flag

    let remove = fun struct (x, y) -> Operations.remove grid x y

    let adjacent = fun struct (x, y) -> Algorithms.adjacent w h x y

    let count f a =
        let mutable n = 0
        for i in adjacent a do
            if get i |> f then
                n <- n + 1
        n

    let choose f a = seq {
        for i in adjacent a do
            if get i |> f then
                yield i
    }

    let reachable i = seq {
        for a in adjacent i do
            for b in choose free a do
                if i <> b then
                    yield b
    }

    let select a b =
        let ta = choose tile a |> Set
        let tb = choose tile b |> Set
        let fa = choose flag a |> Set
        let fb = choose flag b |> Set
        let ua = Set.difference ta fa
        let ub = Set.difference tb fb
        let i = Set.intersect ua ub
        if not (Set.isEmpty i) then
            let ea = Set.difference ua i
            let eb = Set.difference ub i
            let va = get a
            let vb = get b
            let ra = int va - fa.Count
            let rb = int vb - fb.Count
            let ca = ra - ea.Count
            let cb = rb - eb.Count
            let c = max ca cb
            if (c > 0) then
                if not (Set.isEmpty ea) then
                    if ra = ca then
                        ()
                    if c = ra then
                        ea |> Seq.iter (remove >> ignore)
                    if c = ca && Set.isEmpty eb then
                        ea |> Seq.iter (toggle >> ignore)
                if not (Set.isEmpty eb) then
                    if rb = cb then
                        ()
                    if c = rb then
                        eb |> Seq.iter (remove >> ignore)
                    if c = cb && Set.isEmpty ea then
                        eb |> Seq.iter (toggle >> ignore)
        ()

    let invoke () =
        for x = 0 to w - 1 do
            for y = 0 to h - 1 do
                let i = struct (x, y)
                let n = get i
                if (free n && count flag i < int n) then
                    let items = reachable i |> Set
                    for k in items do
                        select i k

    if grid.Status = MineGridStatus.Wait then
        try
            invoke ()
        with
        | MineGridStatusException _ -> ()
    ()
