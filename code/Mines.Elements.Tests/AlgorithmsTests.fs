namespace Mikodev.Mines.Elements.Tests

open Mikodev.Mines.Elements
open Xunit

type AlgorithmsTests() =
    static member ``Array Data`` : obj array seq = seq {
        yield [| box [| for i in 0..4 -> i |] |]
        yield [| box [| for i in 2..9 -> double i |] |]
    }

    [<Theory>]
    [<MemberData(nameof(AlgorithmsTests.``Array Data``))>]
    member __.``ShuffleInPlace`` (source : 'T array) =
        let target = Array.copy source
        Algorithms.shuffleInPlace target
        Assert.Equal<'T>(Array.sort source, Array.sort target)
        ()
        