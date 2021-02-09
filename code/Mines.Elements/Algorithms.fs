module Mikodev.Mines.Elements.Algorithms

open System

/// 洗牌算法 (直接操作数组)
[<CompiledName("ShuffleInPlace")>]
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
