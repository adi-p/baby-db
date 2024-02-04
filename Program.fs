open System

let test store =
    let data =  [ ("a", "a"); ("b", "b"); ("c", "c"); ("d", "d"); ("e", "e"); ]
    let store = 
        data
        |> List.fold (fun acc (k, v) -> MemStore.set acc k v) store
    let tests = [
        ("Should have 'a'", 
            fun (memStore : MemStore.MemStore) -> (MemStore.get memStore "a") = (Some "a"));
        ("Should have 'b'",
            fun (memStore : MemStore.MemStore) -> (MemStore.get memStore "d") = (Some "d")); 
        ("Should not have 'f'",
            fun (memStore : MemStore.MemStore) -> (MemStore.get memStore "f") = None); 
        ("Should still have 'a'",
            fun (memStore : MemStore.MemStore) -> (MemStore.get memStore "a") = (Some "a")); 
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
    test MemStore.empty;
    0

//TODO:

// Base work on -> https://github.com/avinassh/py-caskdb/
// 


// BELOW IS AN OLDER IDEA

// create db
// create/open folder 

// create table
// create/open file

// - open db?

// put item
// new line? (*)

// get item
// scan files

// put item 2 (edit?)

// get item 2 (edit?)

// delete item
// scan and mark? (*) (are we doing this with delete flags?)

// try get item (**) Do we nee this?
//





