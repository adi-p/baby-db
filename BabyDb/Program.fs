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
    
    // let caskStore = CaskStore.empty "caskTest"
    // let caskStore = caskStore |> CaskStore.set "a" "a" |> CaskStore.set "b" "b" |> CaskStore.set "z" "z"
    // caskStore |> CaskStore.print


    // CaskStore.close caskStore
    // let caskStore = CaskStore.load "caskTest"
    // let caskStore = caskStore |> CaskStore.set "a" "a2" |> CaskStore.set "b" "b2"
    // caskStore |> CaskStore.print
    // CaskStore.close caskStore

    // let caskStore = CaskStore.load "caskTest"
    // let caskStore = caskStore |> CaskStore.set "a" "a3" |> CaskStore.delete "z"
    // caskStore |> CaskStore.print
    // CaskStore.close caskStore

    // let caskStore = CaskStore.load "caskTest"
    // caskStore |> CaskStore.print
    // CaskStore.close caskStore


    // use fileStore = new FileStoreOO.FileStoreOO("fileStoreText", false)

    // fileStore.Set "a" "a"
    // fileStore.Print()


    let caskStoreOO = new CaskStoreOO.CaskStoreOO("caskOOTest", false)
    ("a", "a") ||> caskStoreOO.Set  
    ("b", "b") ||> caskStoreOO.Set  
    ("z", "z") ||> caskStoreOO.Set  
    // caskStoreOO.Set "b" "b" 
    // caskStoreOO.Set "z" "z"
    caskStoreOO.Print()

    caskStoreOO.Close()

    let caskStoreOO = new CaskStoreOO.CaskStoreOO("caskOOTest", true)
    caskStoreOO.Print()

    // caskStoreOO.Close()
    caskStoreOO.EraseStore()
    
    0

