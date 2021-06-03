module Mikodev.Mines.Elements.Algorithms

open System

/// 洗牌算法 (直接操作数组)
let shuffle (array : 'T Span) =
    let r = Random()
    for i = 0 to array.Length - 1 do
        let x = r.Next(0, array.Length)
        let a = array.[i]
        array.[i] <- array.[x]
        array.[x] <- a
    ()

/// 展开二维数组索引到一维数组索引
let flatten (w : int) (h : int) =
    if w < 0 || h < 0 then
        raise (ArgumentOutOfRangeException())
    let closure (x : int) (y : int) =
        if uint x > uint w || uint y > uint h then
            raise (ArgumentOutOfRangeException())
        y * w + x
    closure

/// 指定位置周围的索引
let adjacent (w : int) (h : int) (x : int) (y : int) =
    if w < 0 || h < 0 || uint x > uint w || uint y > uint h then
        raise (ArgumentOutOfRangeException())

    let l = if x > 0 then -1 else 0
    let t = if y > 0 then -1 else 0
    let r = if x < w - 1 then 1 else 0
    let b = if y < h - 1 then 1 else 0

    seq {
        for v = t to b do
            for i = l to r do
                if v <> 0 || i <> 0 then
                    yield struct ((x + i), (y + v))
    }
