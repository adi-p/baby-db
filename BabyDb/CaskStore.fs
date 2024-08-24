module CaskStore
  open System
  open System.IO
  open Newtonsoft.Json

  // TODO figure out if you want an functional and OO version again - it's a little bit tedious to keep both

  type FileStoreData = {
    id: int
    // valueSize: int
    // valuePosition: int64
    // TODO - in the future this can have data offsets (for the associated filestore)
  }

  type CaskStore = {
    folderName: string
    // TODO: in the future this dataMap can be the only dataMap (instead of each fileStore having it's own)
    dataMap: Map<string, FileStoreData> 
    openFileStore: FileStore.FileStore
    allFileStores: Map<int, FileStore.FileStore>
    // TODO: do we need a lock? Probably
  }

  // create a folder/cask
  // the cask should keep track of the files/FileStores it owns
  // adding a KV to a cask should add it to the currently open FileStore
  // looking up a KV should find the associated FileStore and fetch it from there
  // create a hintfile at close so loading a cask in the future is easier

  // TODO: look into doing compaction
  // TODO: have a max filestore size
  // TODO: have a lock so that we can ahve multiple readOnly caskstores and only one write one

  let empty (folderName: string) =
    if Directory.Exists(folderName) then
      Directory.Delete(folderName, true)
    Directory.CreateDirectory(folderName) |> ignore
    {
      folderName = folderName;
      dataMap = Map.empty;
      openFileStore = FileStore.empty $"{folderName}/{folderName}" 1
      allFileStores = Map.empty
    }

  let createHintFile (caskStore: CaskStore) =
    let json = caskStore.dataMap |> JsonConvert.SerializeObject
    let fileName = $"{caskStore.folderName}/hintfile"  
    File.WriteAllText (fileName, json)
  
  let loadFromHintFile (caskStore: CaskStore) =
    let dataMap =
      $"{caskStore.folderName}/hintfile" 
      |> File.ReadAllText
      |> JsonConvert.DeserializeObject<Map<string, FileStoreData>>
    {
      caskStore with
        dataMap = dataMap
    }
  
  let private getAllFileStoreNamesAndIds (folderName: string) = 
    Directory.GetFiles(folderName, $"{folderName}*")
    |> Array.map (fun fileName ->
      let i = fileName.IndexOfAny("0123456789".ToCharArray())
      (fileName.Substring(0, i), fileName.Substring(i) |> int))

  let private getLastFileStoreId (folderName: string) = 
    folderName
    |> getAllFileStoreNamesAndIds
    |> Array.map snd // get only ids
    |> Array.max

  let private getFileStoreById (id: int) (caskStore: CaskStore) =
    if id = caskStore.openFileStore.id then
      caskStore.openFileStore
    else 
      caskStore.allFileStores |> Map.find id
      
  let private loadExistingFileStores (folderName: string) =
    folderName
    |> getAllFileStoreNamesAndIds
    |> Array.map (fun (name, id) -> FileStore.load name id)
    |> Array.fold (fun map fileStore -> map |> Map.add fileStore.id fileStore) Map.empty

  let load (folderName: string) = 
    let lastId = folderName |> getLastFileStoreId
    let existingFileStoresMap = folderName |> loadExistingFileStores
    let newFileStore = FileStore.empty $"{folderName}/{folderName}" (lastId + 1)
    {
      folderName = folderName;
      dataMap = Map.empty;
      openFileStore = newFileStore
      allFileStores = existingFileStoresMap
    } |> loadFromHintFile

  let close (caskStore: CaskStore) =
    caskStore |> createHintFile
    caskStore.openFileStore |> FileStore.close
    caskStore.allFileStores |> Map.values |> Seq.iter FileStore.close

  let set (key: string) (value : string) (caskStore: CaskStore) =
    // TODO: when the file gets too big when do we create a new one? here?
    let fileStore = caskStore.openFileStore |> FileStore.set key value
    { 
      caskStore with
        dataMap = caskStore.dataMap |> Map.add key { id = fileStore.id }
        openFileStore = fileStore
    }

  let get (key: string) (caskStore: CaskStore) =
    caskStore.dataMap
    |> Map.tryFind key
    |> Option.bind (fun fileStoreDate -> caskStore |> getFileStoreById fileStoreDate.id |> FileStore.get key)

  let delete (key: string) (caskStore: CaskStore) =
    let fileStore = caskStore.openFileStore |> FileStore.delete key
    { 
      caskStore with
        dataMap = caskStore.dataMap |> Map.remove key
        openFileStore = fileStore
    }

  let keys (caskStore: CaskStore) =
    caskStore.dataMap |> Map.keys 

  let fold folder state (caskStore: CaskStore) =
    caskStore.dataMap 
    |> Map.fold (fun state key fileStore ->
        caskStore
        |> getFileStoreById fileStore.id
        |> FileStore.get key 
        |> Option.get
        |> (folder state key)) 
      state

  let print (caskStore: CaskStore) =
    caskStore
    |> fold (fun _ key value -> printfn "Key: %s Value: %s" key value; ()) ()
    |> ignore


