namespace Mikodev.Mines.Elements

open System

type MineGrid(w : int, h : int, count : int) =
    [<Literal>]
    let Mine : byte = 0xFFuy

    [<Literal>]
    let SizeMax = 128

    do
        if w < 2 || w > SizeMax then
            raise (ArgumentOutOfRangeException(nameof(w)))
        if h < 2 || h > SizeMax then
            raise (ArgumentOutOfRangeException(nameof(h)))
        if count < 1 || count > (w * h - 1) then
            raise (ArgumentOutOfRangeException(nameof(count)))

    let items : byte array = Array.zeroCreate (w * h)

    do
        // 填充 N 个雷, 然后调用洗牌算法
        Array.fill items 0 count Mine
        Algorithms.shuffleInPlace items
        assert(items |> Array.filter ((=) Mine) |> Array.length = count)
