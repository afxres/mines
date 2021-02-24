namespace Mikodev.Mines.Elements

open Mikodev.Mines.Annotations
open System

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

    let adjacent f x y = Algorithms.adjacent w h x y |> Seq.map ((<||) f)

    let calculate array item x y = adjacent flatten x y |> Seq.map (Array.get array) |> Seq.filter ((=) item) |> Seq.length

    let generate i =
        // 调用洗牌算法并忽略最后一个位置
        let result : byte array = Array.zeroCreate (w * h)
        let last = result.Length - 1
        Array.fill result 0 count Mine
        Algorithms.shuffleInPlace (result.AsSpan(0, last))

        // 交换第一次点击的位置和最后一个位置
        let x = result.[last]
        result.[last] <- result.[i]
        result.[i] <- x

        // 计算每个位置周围的雷个数
        for m = 0 to w - 1 do
            for n = 0 to h - 1 do
                let i = flatten m n
                if (result.[i] <> Mine) then
                    result.[i] <- byte (calculate result Mine m n)
                ()

        assert (result.[i] <> Mine)
        result

    let status = Event<_, _>()

    let face : TileMark array = Array.create (w * h) TileMark.Tile

    let mutable step = MineGridStatus.None

    let mutable back : byte array = null

    let mutable miss = -1

    let mutable tile = w * h

    let mutable flag = 0

    let update x = step <- x; status.Trigger(me, EventArgs.Empty)

    // 递归翻开周围方块
    let rec remove x y =
        let i = flatten x y
        let m = &face.[i]
        match m with
        | TileMark.Tile ->
            m <- TileMark.None
            match back.[i] with
            | 0uy -> adjacent remove x y |> Seq.sum |> (+) 1
            | Mine -> miss <- i; 1
            | _ -> 1
        | _ -> 0

    let finish n =
        assert (n <= tile && n >= 0)
        tile <- tile - n
        assert (step = MineGridStatus.Wait)
        if miss <> -1 then
            update MineGridStatus.Over
        elif tile = count then
            update MineGridStatus.Done
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
            let i = flatten x y
            if step = MineGridStatus.None then
                back <- generate i
                update MineGridStatus.Wait
            elif step <> MineGridStatus.Wait then
                invalidOp "Game is over!"
            remove x y |> finish
