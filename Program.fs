open System

// This work is based on -> https://github.com/avinassh/py-caskdb/

let testMemStore store =
    printfn "Testing MemStore"
    let data =  [ ("a", "a"); ("b", "b"); ("c", "c"); ("d", "d"); ("e", "e"); ]
    let store = 
        data
        |> List.fold (fun acc (k, v) -> MemStore.set acc k v) store
    let tests = [
        ("Should have 'a'", 
            fun (memStore : MemStore.MemStore) -> (MemStore.get memStore "a") = (Some "a"));
        ("Should have 'd'",
            fun (memStore : MemStore.MemStore) -> (MemStore.get memStore "d") = (Some "d")); 
        ("Should not have 'f'",
            fun (memStore : MemStore.MemStore) -> (MemStore.get memStore "f") = None); 
        ("Should still have 'a'",
            fun (memStore : MemStore.MemStore) -> (MemStore.get memStore "a") = (Some "a")); 
        ("Should replace 'a'",
            fun (memStore : MemStore.MemStore) ->
                let memStore = (MemStore.set memStore "a" "a-replace") // TODO: change function contract so that chaining is easier
                (MemStore.get memStore "a") = (Some "a-replace")
        ); 
    ]
    let pass =
        tests
        |> List.fold 
            (fun resultAcc (testName, testFunc) -> 
                let result = testFunc store
                printfn "%s - %b" testName result
                result && resultAcc
            )
            true
    if pass then
        printfn "All tests pass"
    else 
        printfn "Not all tests pass"


let testFileStore store =
    printfn "Testing FileStore"
    let data =  [ ("a", "a"); ("b", "bb"); ("c", "ccc"); ("d", "dd"); ("e", "e"); ]
    let store = 
        data
        |> List.fold (fun acc (k, v) -> FileStore.set acc k v) store

    // printfn "DB bytes"
    // printfn "%A" ("testDB" |> System.IO.File.ReadAllBytes |> System.Text.Encoding.ASCII.GetString)
    // printfn "%s" (FileStore.get store "a")
    // printfn "%s" (FileStore.get store "b")
    // printfn "%s" (FileStore.get store "c")
    // printfn "%s" (FileStore.get store "d")
    // printfn "%s" (FileStore.get store "e")

    // TODO: write tests that do multiple things in a row rather than one thing per test

    let tests = [
        ("Should have 'a'", 
            fun (fileStore : FileStore.FileStore) -> (FileStore.get fileStore "a") = (Some "a"));
        ("Should have 'c'",
            fun (fileStore : FileStore.FileStore) -> (FileStore.get fileStore "c") = (Some "ccc")); 
        ("Should have 'd'",
            fun (fileStore : FileStore.FileStore) -> (FileStore.get fileStore "d") = (Some "dd")); 
        ("Should not have 'f'",
            fun (fileStore : FileStore.FileStore) -> (FileStore.get fileStore "f") = None); 
        ("Should still have 'a'",
            fun (fileStore : FileStore.FileStore) -> (FileStore.get fileStore "a") = (Some "a"));
        ("Should replace 'a'",
            fun (fileStore : FileStore.FileStore) ->
                let fileStore = (FileStore.set fileStore "a" "a-replace") // TODO: change function contract so that chaining is easier
                (FileStore.get fileStore "a") = (Some "a-replace")
        ); 
    ]
    let pass =
        tests
        |> List.fold 
            (fun resultAcc (testName, testFunc) -> 
                let result = testFunc store
                printfn "%s - %b" testName result
                result && resultAcc
            )
            true
    if pass then
        printfn "All tests pass"
    else 
        printfn "Not all tests pass"


[<EntryPoint>]
let main args = 
    testMemStore MemStore.empty;
    testFileStore (FileStore.empty "testDB")
    0





