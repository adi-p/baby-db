open System

// This work is based on -> https://github.com/avinassh/py-caskdb/

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
          (memStore |> MemStore.set "a" "a-replace" |> MemStore.get "a") = (Some "a-replace")) ]

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


let testFileStore store =
    printfn "Testing FileStore"
    let data = [ ("a", "a"); ("b", "bb"); ("c", "ccc"); ("d", "dd"); ("e", "e") ]

    let store =
      data |> List.fold (fun accStore (k, v) -> accStore |> FileStore.set k v) store

    // printfn "DB bytes"
    // printfn "%A" ("testDB" |> System.IO.File.ReadAllBytes |> System.Text.Encoding.ASCII.GetString)
    // printfn "%s" (FileStore.get store "a")
    // printfn "%s" (FileStore.get store "b")
    // printfn "%s" (FileStore.get store "c")
    // printfn "%s" (FileStore.get store "d")
    // printfn "%s" (FileStore.get store "e")

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
            (fileStore |> FileStore.set "a" "a-replace" |> FileStore.get "a") = (Some "a-replace")) ]

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


[<EntryPoint>]
let main args =
  testMemStore MemStore.empty
  testFileStore (FileStore.empty "testDB")
  0
