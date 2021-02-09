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
    // 判断上下左右是否存在
    let l = x > 0
    let r = x < w - 1
    let t = y > 0
    let b = y < h - 1

    // 依次枚举左中右三个元素
    let action line = seq {
        if l then
            yield f (x - 1) line
        // 排除自身
        if y <> line then
            yield f x line
        if r then
            yield f (x + 1) line
    }

    // 依次枚举上中下三行
    seq {
        if t then
            yield! action (y - 1)
        yield! action y
        if b then
            yield! action (y + 1)
    }
