namespace Mikodev.Mines.Elements

open Mikodev.Mines.Annotations
open System
open System.Collections.Generic

type MineGrid(w : int, h : int, count : int) as me =
    [<Literal>]
    let Mine : byte = 0xFFuy

    [<Literal>]
    let SizeMax = 1024

    let ensure min max value name =
        if value < min || value > max then
            raise (ArgumentOutOfRangeException name)

    do
        ensure 2 SizeMax w (nameof w)
        ensure 2 SizeMax h (nameof h)
        ensure 1 (w * h - 1) count (nameof count)

    let flatten = Algorithms.flatten w h

    let generate x y =
        let select a x y = if Array.get a (flatten x y) = Mine then 1 else 0
        let detect a x y = Algorithms.adjacent w h x y |> Seq.sumBy ((<||) (select a))

        // 调用洗牌算法并忽略最后一个位置
        let data : byte array = Array.zeroCreate (w * h)
        let last = data.Length - 1
        Array.fill data 0 count Mine
        Algorithms.shuffleInPlace (data.AsSpan(0, last))

        // 交换第一次点击的位置和最后一个位置
        let i = flatten x y
        let k = data.[last]
        data.[last] <- data.[i]
        data.[i] <- k

        // 计算每个位置周围的雷个数
        for x = 0 to w - 1 do
            for y = 0 to h - 1 do
                let m = &data.[flatten x y]
                if (m <> Mine) then
                    m <- detect data x y |> byte

        assert (data.[i] <> Mine)
        data

    let status = Event<_, _>()

    let face : TileMark array = Array.create (w * h) TileMark.Tile

    let mutable step = MineGridStatus.None

    let mutable back : byte array = null

    let mutable miss = -1

    let mutable tile = w * h

    let mutable flag = 0

    let update x = step <- x; status.Trigger(me, EventArgs.Empty)

    // 移除方块 (原递归方法可能会栈溢出, 此处改用开闭列表)
    let remove x y =
        let adjacent = Algorithms.adjacent w h
        let mutable o = List<_>(Seq.singleton (x, y))
        let mutable c = HashSet<_>()
        let mutable n = 0
        while o.Count > 0 do
            let t = o.Count - 1
            let p = o.[t]
            o.RemoveAt t
            if c.Add p then
                let a, b = p
                let i = flatten a b
                let m = &face.[i]
                if (m = TileMark.Tile) then
                    m <- TileMark.None
                    n <- n + 1
                    match back.[i] with
                    | 0uy -> adjacent a b |> Seq.iter o.Add
                    | Mine -> miss <- i
                    | _ -> ()
        n

    interface IMineGrid with
        [<CLIEvent>]
        member __.StatusChanged = status.Publish

        member __.Status: MineGridStatus = step

        member __.XMax = w

        member __.YMax = h

        member __.FlagCount = flag

        member __.MineCount = count

        member __.Get(x, y) =
            let i = flatten x y
            let b = if back |> isNull then -1 else int back.[i]
            let m = b = int Mine
            let f = step = MineGridStatus.Over
            match face.[i] with
            | TileMark.Tile -> if f && m then MineData.Mine else MineData.Tile
            | TileMark.Flag -> if f && not m then MineData.FlagMiss else MineData.Flag
            | TileMark.What -> if f && not m then MineData.WhatMiss else MineData.What
            | _ -> if i = miss then MineData.MineMiss elif m then MineData.Mine else enum<MineData> b

        member __.Set(x, y, mark) =
            if step <> MineGridStatus.None && step <> MineGridStatus.Wait then
                invalidOp "Can not operate now!"

            let t =
                match mark with
                | MineMark.None -> TileMark.Tile
                | MineMark.Flag -> TileMark.Flag
                | MineMark.What -> TileMark.What
                | _ -> invalidArg (nameof mark) "Invalid mine mark!"

            let i = flatten x y
            let s = face.[i]
            if (s <> t) then
                face.[i] <- t
                if s = TileMark.Flag then
                    flag <- flag - 1
                elif t = TileMark.Flag then
                    flag <- flag + 1
            ()

        member __.Remove(x, y) =
            if step = MineGridStatus.None then
                back <- generate x y
                update MineGridStatus.Wait
            elif step <> MineGridStatus.Wait then
                invalidOp "Game is over!"

            let n = remove x y
            assert (n <= tile && n >= 0)
            tile <- tile - n
            assert (step = MineGridStatus.Wait)
            if miss <> -1 then
                update MineGridStatus.Over
            elif tile = count then
                update MineGridStatus.Done
            n
