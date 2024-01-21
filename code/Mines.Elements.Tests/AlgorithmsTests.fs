namespace Mikodev.Mines.Elements.Tests

open Mikodev.Mines.Elements
open System
open Xunit

type AlgorithmsTests() =
    static member ``Index Data`` : obj array seq = seq {
        let tie w h x y array = [| box w; box h; box x; box y; box array |]

        yield (tie 1 1 0 0 Array.empty<(int * int)>)
        yield (tie 9 1 1 0 [| 0, 0; 2, 0 |])
        yield (tie 1 7 0 2 [| 0, 1; 0, 3 |])
        yield (tie 9 9 0 0 [| 1, 0; 0, 1; 1, 1 |])
        yield (tie 8 8 7 0 [| 6, 0; 6, 1; 7, 1 |])
        yield (tie 6 6 0 5 [| 0, 4; 1, 4; 1, 5 |])
        yield (tie 9 9 8 8 [| 7, 7; 8, 7; 7, 8 |])
        yield (tie 7 7 3 3 [| 2, 2; 3, 2; 4, 2; 2, 3; 4, 3; 2, 4; 3, 4; 4, 4 |])
    }

    [<Theory>]
    [<MemberData(nameof AlgorithmsTests.``Index Data``)>]
    member __.``Adjacent`` (w : int, h : int, x : int, y : int, expected : (int * int) array) =
        let result = Algorithms.adjacent w h x y |> Seq.toArray
        let select = fun (a, b) -> struct (a, b)
        Assert.Equal<struct (int * int)>(expected |> Array.map select, result)
        ()

    [<Theory>]
    [<InlineData(-1, 0)>]
    [<InlineData(0, -1)>]
    member __.``Flatten (argument error)`` (w, h) =
        let error = Assert.Throws<ArgumentOutOfRangeException>(fun () -> Algorithms.flatten w h |> ignore)
        Assert.Null error.ParamName
        Assert.Equal(error.Message, ArgumentOutOfRangeException().Message)
        ()

    [<Theory>]
    [<InlineData(1, 1, 0, 1)>]
    [<InlineData(1, 1, 1, 0)>]
    [<InlineData(1, 1, 0, -1)>]
    [<InlineData(1, 1, -1, 0)>]
    member __.``Flatten (closure argument error)`` (w, h, x, y) =
        let closure = Algorithms.flatten w h
        let error = Assert.Throws<ArgumentOutOfRangeException>(fun () -> closure x y |> ignore)
        Assert.Null error.ParamName
        Assert.Equal(error.Message, ArgumentOutOfRangeException().Message)
        ()
