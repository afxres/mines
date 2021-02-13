module Mikodev.Mines.Elements.Algorithms

open System

/// 洗牌算法 (直接操作数组)
let shuffleInPlace (array : 'T array) =
    if array |> isNull then
        raise (ArgumentNullException(nameof(array)))
    let r = Random()
    for i = 0 to array.Length - 1 do
        let x = r.Next(0, array.Length)
        let a = array.[i]
        array.[i] <- array.[x]
        array.[x] <- a
    ()

/// 展开二维数组索引到一维数组索引
let flattenIndex (w : int) (h : int) (x : int) (y : int) =
    if w < 0 || h < 0 || uint x > uint w || uint y > uint h then
        raise (ArgumentOutOfRangeException())
    y * w + x

/// 映射指定位置周围的索引
let mapAdjacentIndexes (w : int) (h : int) (x : int) (y : int) (f : int -> int -> 'T) =
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
                    yield f (x + i) (y + v)
    }
