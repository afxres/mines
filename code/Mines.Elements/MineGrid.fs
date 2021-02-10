namespace Mikodev.Mines.Elements

open System

type MineGrid(w : int, h : int, count : int) =
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

    let generate x y =
        ensure 0 (w - 1) x (nameof x)
        ensure 0 (h - 1) y (nameof y)

        // 创建一个比总数小 1 的数组, 填充 N 个雷, 然后调用洗牌算法
        let origin : byte array = Array.zeroCreate (w * h - 1)
        Array.fill origin 0 count Mine
        Algorithms.shuffleInPlace origin
        let i = Algorithms.flattenIndex w h x y
        // 向数组的指定位置 (第一次翻开的位置) 插入空元素
        let result = Array.concat (seq {
            origin.[0..i]
            Array.singleton 0uy
            origin.[(i + 1)..]
        })
        assert(result.Length = w * h)

        let flatten = Algorithms.flattenIndex w h

        for m = 0 to w - 1 do
            for n = 0 to h - 1 do
                let i = flatten m n
                if (result.[i] <> Mine) then
                    let s = Algorithms.mapAdjacentIndexes w h m n flatten
                    let n = s |> Seq.filter (fun x -> result.[x] = Mine) |> Seq.length
                    result.[i] <- byte n
                ()

        assert(result |> Array.filter ((=) Mine) |> Array.length = count)
        result
