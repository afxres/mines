namespace Mikodev.Mines.Elements

open Mikodev.Mines.Annotations
open System
open System.ComponentModel

type MineGrid(w : int, h : int, count : int) as me =
    [<Literal>]
    let Mine : byte = 0xFFuy

    [<Literal>]
    let SizeMax = 128

    let ensure min max value name =
        if value < min || value > max then
            raise (ArgumentOutOfRangeException name)

    do
        ensure 2 SizeMax w (nameof w)
        ensure 2 SizeMax h (nameof h)
        ensure 1 (w * h - 1) count (nameof count)

    let flatten = Algorithms.flattenIndex w h

    let adjacent = Algorithms.mapAdjacentIndexes w h

    let calculate array item x y = adjacent x y flatten |> Seq.map (Array.get array) |> Seq.filter ((=) item) |> Seq.length

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
        assert (result |> Seq.filter ((=) Mine) |> Seq.length = count)
        result

    let notify = Event<_, _>()

    let face : TileMark array = Array.create (w * h) TileMark.Tile

    let mutable step = MineGridStatus.None

    let mutable back : byte array = null

    let mutable miss = -1

    let mutable tile = w * h

    let mutable flag = 0

    let update (item : 'a byref) data name =
        assert (typeof<IMineGrid>.GetProperty name <> null)
        assert (item <> data)
        item <- data
        notify.Trigger(me, PropertyChangedEventArgs name)
        ()

    let step' x = update &step x "Status"

    let flag' x = update &flag x "FlagCount"

    // 递归翻开周围方块
    let rec remove x y =
        let i = flatten x y
        let m = &face.[i]
        match m with
        | TileMark.Tile ->
            m <- TileMark.None
            match back.[i] with
            | 0uy -> adjacent x y remove |> Seq.sum |> (+) 1
            | Mine -> miss <- i; 1
            | _ -> 1
        | _ -> 0

    let finish n =
        assert (n <= tile && n >= 0)
        tile <- tile - n
        assert (face |> Seq.filter ((<>) TileMark.None) |> Seq.length = tile)
        assert (step = MineGridStatus.Wait)
        if miss <> -1 then
            step' MineGridStatus.Over
        elif tile = count then
            step' MineGridStatus.Done
        n

    interface IMineGrid with
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

        member __.Set(x, y) =
            if step <> MineGridStatus.None && step <> MineGridStatus.Wait then
                invalidOp "Can not operate now!"

            let i = flatten x y
            let m = &face.[i]
            match m with
            | TileMark.Tile -> m <- TileMark.Flag; flag' (flag + 1)
            | TileMark.Flag -> m <- TileMark.What; flag' (flag - 1)
            | TileMark.What -> m <- TileMark.Tile
            | _ -> ()
            assert (face |> Seq.filter ((=) TileMark.Flag) |> Seq.length = flag)
            ()

        member __.Remove(x, y) =
            let i = flatten x y
            if step = MineGridStatus.None then
                back <- generate i
                step' MineGridStatus.Wait
            elif step <> MineGridStatus.Wait then
                invalidOp "Game is over!"
            remove x y |> finish

        member __.RemoveAll(x, y) =
            if step <> MineGridStatus.Wait then
                invalidOp "Can not operate now!"

            let drop a b =
                let i = flatten a b
                let m = &face.[i]
                if (m = TileMark.What) then m <- TileMark.Tile
                remove a b

            let i = flatten x y
            let n = int back.[i]
            if face.[i] = TileMark.None && n <> 0 && calculate face TileMark.Flag x y >= n then
                adjacent x y drop |> Seq.sum |> finish
            else
                0

    interface INotifyPropertyChanged with
        [<CLIEvent>]
        member __.PropertyChanged = notify.Publish
