namespace Mikodev.Mines.Elements.Tests

open Mikodev.Mines.Elements
open System
open Xunit

type AlgorithmsTests() =
    static member ``Array Data`` : obj array seq = seq {
        yield [| box [| for i in 0..4 -> i |] |]
        yield [| box [| for i in 2..9 -> double i |] |]
    }

    [<Theory>]
    [<MemberData(nameof AlgorithmsTests.``Array Data``)>]
    member __.``ShuffleInPlace`` (source : 'T array) =
        let target = Array.copy source
        Algorithms.shuffleInPlace (Span target)
        Assert.Equal<'T>(Array.sort source, Array.sort target)
        ()

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
        Assert.Equal<(int * int)>(expected, result)
        ()
