﻿namespace Mikodev.Mines.Elements

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

    let generate i =
        // 创建一个比总数小 1 的数组, 填充 N 个雷, 然后调用洗牌算法
        let origin : byte array = Array.zeroCreate (w * h - 1)
        Array.fill origin 0 count Mine
        Algorithms.shuffleInPlace origin
        // 向数组的指定位置 (第一次翻开的位置) 插入空元素
        let slices = [|
            origin.[0..(i - 1)]
            Array.singleton 0uy
            origin.[i..]
        |]
        let result = Array.concat slices
        assert(result.Length = w * h)

        for m = 0 to w - 1 do
            for n = 0 to h - 1 do
                let i = flatten m n
                if (result.[i] <> Mine) then
                    let s = adjacent m n flatten
                    let n = s |> Seq.filter (fun x -> result.[x] = Mine) |> Seq.length
                    result.[i] <- byte n
                ()

        assert(result.[i] <> Mine)
        assert(result |> Seq.filter ((=) Mine) |> Seq.length = count)
        result

    let changed = Event<_, _>()

    let face : TileMark array = Array.create (w * h) TileMark.Tile

    let mutable over = false

    let mutable good = false

    let mutable back : byte array = null

    let mutable miss = -1

    let mutable tile = w * h

    let mutable flag = 0

    let change (item : 'a byref) data name =
        assert(typeof<IMineGrid>.GetProperty name <> null)
        if (item <> data) then
            item <- data
            changed.Trigger(me, PropertyChangedEventArgs name)
        ()

    let validate () =
        if over then
            invalidOp "Game is over!"

    // 递归翻开周围方块
    let rec remove x y =
        let i = flatten x y
        let m = &face.[i]
        match m with
        | TileMark.Tile ->
            m <- TileMark.None
            match back.[i] with
            | 0uy -> adjacent x y remove |> Seq.sum |> (+) 1
            | Mine ->
                miss <- i
                change &over true "IsOver"
                1
            | _ -> 1
        | _ -> 0

    let finish n =
        assert(n <= tile && n >= 0)
        tile <- tile - n
        assert(face |> Seq.filter ((<>) TileMark.None) |> Seq.length = tile)
        if not over && tile = count then
            change &good true "IsDone"
        ()

    interface IMineGrid with
        member __.Status: MineGridStatus = raise (NotImplementedException())

        member __.IsDone = good

        member __.IsOver = over

        member __.XMax = w

        member __.YMax = h

        member __.FlagCount = flag

        member __.MineCount = count

        member __.Get(x, y) =
            let i = flatten x y
            let b = if back |> isNull then -1 else int back.[i]
            let m = b = int Mine
            match face.[i] with
            | TileMark.Tile -> if over && m then MineData.Mine else MineData.Tile
            | TileMark.Flag -> if over && not m then MineData.FlagMiss else MineData.Flag
            | TileMark.What -> if over && not m then MineData.WhatMiss else MineData.What
            | _ ->
                if m then
                    if i <> miss then MineData.Mine else MineData.MineMiss
                else
                    enum<MineData>(b)

        member __.Set(x, y) =
            validate()
            let i = flatten x y
            let m = &face.[i]
            match m with
            | TileMark.Tile -> m <- TileMark.Flag; change &flag (flag + 1) "FlagCount"
            | TileMark.Flag -> m <- TileMark.What; change &flag (flag - 1) "FlagCount"
            | TileMark.What -> m <- TileMark.Tile
            | _ -> ()
            assert(face |> Seq.filter ((=) TileMark.Flag) |> Seq.length = flag)
            ()

        member __.Remove(x, y) =
            validate()
            let i = flatten x y
            // 第一次点击时生成地图
            if (back |> isNull) then
                back <- generate i
            remove x y |> finish
            ()

        member __.RemoveAll(x, y) =
            validate()

            let drop a b =
                let i = flatten a b
                let m = &face.[i]
                if (m = TileMark.What) then m <- TileMark.Tile
                remove a b

            let i = flatten x y
            let n = int back.[i]
            if face.[i] = TileMark.None && n <> 0 then
                let flags = adjacent x y flatten |> Seq.map (Array.get face) |> Seq.filter ((=) TileMark.Flag)
                if (flags |> Seq.length) >= n then
                    adjacent x y drop |> Seq.sum |> finish
            ()

    interface INotifyPropertyChanged with
        [<CLIEvent>]
        member __.PropertyChanged = changed.Publish
