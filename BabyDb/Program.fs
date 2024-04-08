open System

// let largeTest n fileStore =
//     (seq { for i in 1..n do yield  i.ToString() } : string seq)
//     |> Seq.fold (fun store i -> FileStore.set i i store) fileStore
//     |> FileStore.get (n.ToString())

// let largeTestOO n (fileStore: FileStore.FileStoreOO) =
//     (seq { for i in 1..n do yield  i.ToString() } : string seq)
//     |> Seq.fold (fun store i -> fileStore.Set i i) ()
    
//     fileStore.Get (n.ToString())

[<EntryPoint>]
let main args =
    // largeTest 100000 (FileStore.empty "testDB")
    // |> Option.defaultValue "wrong - Null"
    // |> printfn "%s"
    0

