open System

// This work is based on -> https://github.com/avinassh/py-caskdb/
// py-caskdb is in turn based on 'bitcask' -> https://riak.com/assets/bitcask-intro.pdf

let testMemStore store =
    printfn "Testing MemStore"
    let data = [ ("a", "a"); ("b", "b"); ("c", "c"); ("d", "d"); ("e", "e") ]

    let store =
        data |> List.fold (fun accStore (k, v) -> accStore |> MemStore.set k v) store

    let tests =
        [ ("Should have 'a'", (fun (memStore: MemStore.MemStore) -> memStore |> (MemStore.get "a") = (Some "a")))
          ("Should have 'd'", (fun (memStore: MemStore.MemStore) -> memStore |> (MemStore.get "d") = (Some "d")))
          ("Should not have 'f'", (fun (memStore: MemStore.MemStore) -> memStore |> (MemStore.get "f") = None))
          ("Should still have 'a'", (fun (memStore: MemStore.MemStore) -> memStore |> (MemStore.get "a") = (Some "a")))
          ("Should replace 'a'",
           fun (memStore: MemStore.MemStore) ->
               (memStore |> MemStore.set "a" "a-replace" |> MemStore.get "a") = (Some "a-replace"))
          ("Should remove 'a'",
           fun (memStore: MemStore.MemStore) -> (memStore |> MemStore.delete "a" |> MemStore.get "a") = None) ]

    let pass =
        tests
        |> List.fold
            (fun resultAcc (testName, testFunc) ->
                let result = testFunc store
                printfn "%s - %b" testName result
                result && resultAcc)
            true

    if pass then
        printfn "All tests pass"
    else
        printfn "Not all tests pass"


let createFileStore store =
    let data = [ ("a", "a"); ("b", "bb"); ("c", "ccc"); ("d", "dd"); ("e", "e") ]
    data |> List.fold (fun accStore (k, v) -> accStore |> FileStore.set k v) store

let testFileStore store =
    printfn "Testing FileStore"
    // let store = Option.defaultValue (createFileStore (FileStore.empty "testDB")) store 
      

    // printfn "DB bytes"
    // printfn "%A" ("testDB" |> System.IO.File.ReadAllBytes |> System.Text.Encoding.ASCII.GetString)

    // TODO: write tests that do multiple things in a row rather than one thing per test

    let tests =
        [ ("Should have 'a'", (fun (fileStore: FileStore.FileStore) -> (fileStore |> FileStore.get "a") = (Some "a")))
          ("Should have 'c'", (fun (fileStore: FileStore.FileStore) -> (fileStore |> FileStore.get "c") = (Some "ccc")))
          ("Should have 'd'", (fun (fileStore: FileStore.FileStore) -> (fileStore |> FileStore.get "d") = (Some "dd")))
          ("Should not have 'f'", (fun (fileStore: FileStore.FileStore) -> (fileStore |> FileStore.get "f") = None))
          ("Should still have 'a'",
           fun (fileStore: FileStore.FileStore) -> (fileStore |> FileStore.get "a") = (Some "a"))
          ("Should replace 'a'",
           fun (fileStore: FileStore.FileStore) ->
               (fileStore |> FileStore.set "a" "a-replace" |> FileStore.get "a") = (Some "a-replace"))
          ("Should remove 'a'",
           fun (fileStore: FileStore.FileStore) -> (fileStore |> FileStore.delete "a" |> FileStore.get "a") = None) 
        ]

    let pass =
        tests
        |> List.fold
            (fun resultAcc (testName, testFunc) ->
                let result = testFunc store
                printfn "%s - %b" testName result
                result && resultAcc)
            true

    if pass then
        printfn "All tests pass"
    else
        printfn "Not all tests pass"


// let largeTest n fileStore =
//     (seq { for i in 1..n do yield  i.ToString() } : string seq)
//     |> Seq.fold (fun store i -> FileStore.set i i store) fileStore
//     |> FileStore.get (n.ToString())

[<EntryPoint>]
let main args =
    testMemStore MemStore.empty
    testFileStore (createFileStore (FileStore.empty "testDB"))
    printfn "-- Load test --" // TODO: write tests for 'load' seperately
    testFileStore (FileStore.load "testDB")
    // largeTest 100000(FileStore.empty "testDB")
    // |> Option.defaultValue "wrong - Null"
    // |> printfn "%s"
    0
