module CaskStoreOO
  open System
  open System.IO
  open Newtonsoft.Json
  open FileStoreOO

  let ifNoneOrFalse (f: unit -> unit) (option:Option<bool>) =
    match option with 
    | None
    | Some false -> f ()
    | Some true -> ()
  
  // TODO figure out if you want a functional and OO version again - it's a little bit tedious to keep both

  type FileStoreData = {
    id: int
    // valueSize: int
    // valuePosition: int64
    // TODO - in the future this can have data offsets (for the associated filestore)
  }

  type CaskStoreOO(folderName, load) = 
    let folderName = folderName

    let getAllFileStoreNamesAndIds (folderName: string) = 
      Directory.GetFiles(folderName, $"{folderName}*")
      |> Array.map (fun fileName ->
        let i = fileName.IndexOfAny("0123456789".ToCharArray())
        (fileName.Substring(0, i), fileName.Substring(i) |> int))

    let getLastFileStoreId (folderName: string) = 
      folderName
      |> getAllFileStoreNamesAndIds
      |> Array.map snd // get only ids
      |> Array.max

    // TODO: in the future this dataMap can be the only dataMap (instead of each fileStore having it's own)
    let mutable dataMap = Map.empty
    
    let mutable (openFileStore : Option<FileStoreOO>) = None

    let mutable allFileStores = Map.empty
    // TODO: do we need a lock? Probably

    // create a folder/cask
    // the cask should keep track of the files/FileStores it owns
    // adding a KV to a cask should add it to the currently open FileStore
    // looking up a KV should find the associated FileStore and fetch it from there
    // create a hintfile at close so loading a cask in the future is easier

    // TODO: look into doing compaction
    // TODO: have a max filestore size
    // TODO: have a lock so that we can ahve multiple readOnly caskstores and only one write one

    let loadExistingFileStores (folderName: string) =
      folderName
      |> getAllFileStoreNamesAndIds
      |> Array.map (fun (name, id) -> new FileStoreOO(name, id, true))
      |> Array.fold (fun map fileStore -> map |> Map.add fileStore.Id fileStore) Map.empty

    do
      if load then
        allFileStores <- folderName |> loadExistingFileStores
        openFileStore <- Some (new FileStoreOO($"{folderName}/{folderName}", (folderName |> getLastFileStoreId |> (+) 1), false))
        dataMap <- $"{folderName}/hintfile" 
          |> File.ReadAllText
          |> JsonConvert.DeserializeObject<Map<string, FileStoreData>>
      else 
        if Directory.Exists(folderName) then
          Directory.Delete(folderName, true)
        Directory.CreateDirectory(folderName) |> ignore
        openFileStore <- Some (new FileStoreOO($"{folderName}/{folderName}", 1, false))

    let getFileStoreById (id: int) =
      match openFileStore with
      | Some openFileStore when openFileStore.Id = id -> openFileStore
      | _ ->  allFileStores |> Map.find id
       

    let createHintFile () =
      let json = dataMap |> JsonConvert.SerializeObject
      let fileName = $"{folderName}/hintfile"  
      File.WriteAllText (fileName, json)

    // TODO check if this really makes sense
    let getOpenFileStore =
      match openFileStore with
        | Some fs -> fs
        | _ ->
          openFileStore <- Some (new FileStoreOO($"{folderName}/{folderName}", (folderName |> getLastFileStoreId |> (+) 1), false))
          Option.get openFileStore

    member this.Close(?skipHintFile) =
      skipHintFile |> ifNoneOrFalse (createHintFile)
      openFileStore |> Option.iter (fun openFileStore -> openFileStore.Close())
      allFileStores |> Map.values |> Seq.iter (fun fs -> fs.Close())

    member this.EraseStore() =
      this.Close(true)
      Directory.Delete(folderName, true)
    
    interface IDisposable with
        member this.Dispose() = 
            this.Close()

    member this.Set (key: string) (value : string) =
      // TODO: when the file gets too big when do we create a new one? here?
      // TODO: check if recreating makes sense
      let newFileStore = (getOpenFileStore |> fun fs -> fs.Set key value; fs)
      openFileStore <- Some newFileStore
      dataMap <- dataMap |> Map.add key { id = newFileStore.Id }

    member this.Get (key: string) =
      dataMap
      |> Map.tryFind key
      |> Option.bind (fun fileStore -> getFileStoreById fileStore.id |> fun fs -> fs.Get key)

    member this.Delete (key: string) =
      (getOpenFileStore |> fun fs -> fs.Delete key)
      dataMap <- dataMap |> Map.remove key

    member this.Keys = dataMap |> Map.keys 

    member this.Fold folder state =
      dataMap 
      |> Map.fold (fun state key fileStore ->
          getFileStoreById fileStore.id
          |> fun fs ->
            fs.Get key 
            |> Option.get
            |> (folder state key)) 
        state

     member this.Print ()  =
      this.Fold (fun _ key value -> printfn "Key: %s Value: %s" key value; ()) ()
      |> ignore


