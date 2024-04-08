
module FileStore
  open System
  open System.IO

  // FILE FORMATTING

  // IDEAS:
  // Need to set the format for the KV pair and the header
  // Cask_db does: 
  // ┌───────────┬──────────┬────────────┬─────┬───────┐
  // │ timestamp │ key_size │ value_size │ key │ value │
  // └───────────┴──────────┴────────────┴─────┴───────┘
  // ┌───────────────┬──────────────┬────────────────┐
  // │ timestamp(4B) │ key_size(4B) │ value_size(4B) │
  // └───────────────┴──────────────┴────────────────┘

  let HEADER_SIZE = sizeof<int32> * 2

  let (bytesToString : byte[] -> string) = System.Text.Encoding.ASCII.GetString

  let encodeKV timestamp (key: string) (value: byte[]) =
    // TODO: use timestamp!
    let keyBytes = key |> System.Text.Encoding.ASCII.GetBytes
    let keySize = keyBytes |> Array.length |> BitConverter.GetBytes 
    let valueSize = value |> Array.length |> BitConverter.GetBytes
    let byteArray = seq { keySize; valueSize; keyBytes; value; } |> Array.concat
    byteArray |> Array.length , byteArray
  
  let decodeKV (bytes: byte[]) =
    // Get key size, get value size, read key, read value, return
    // TODO: use timestamp
    let keySize = bytes[0..3] |> BitConverter.ToInt32
    let valueSize = bytes[4..7] |> BitConverter.ToInt32
    let key = bytes[HEADER_SIZE..(HEADER_SIZE + keySize-1)] |> bytesToString
    let value = bytes[(HEADER_SIZE + keySize)..(HEADER_SIZE + keySize+valueSize-1)] |> bytesToString
    (0, key, value)


  type FileStore = {
    fileName : string
    dataMap : Map<string, int*int64> // a map from key to (byte length, byte offset in file)
    writePosition: int64
    readFileStream: FileStream
    writeFileStream: FileStream
    // TODO consider storing the fileStream here. Maybe one for read and one for write
  }

  let empty fileName = 
    // TODO: check if file already exists?
    { 
      dataMap = Map.empty; 
      fileName = fileName; 
      writePosition = 0;
      writeFileStream = File.Open (fileName, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
      readFileStream = File.Open (fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
    }

  let close (fileStore: FileStore) =
    if fileStore.writeFileStream.CanWrite then
      fileStore.writeFileStream.Flush true
    fileStore.readFileStream.Close ()
    fileStore.writeFileStream.Close ()

  let load fileName =
    // Find the current file and create the dataMap and set the writePosition
    // Read first 8 bytes (keySize and valueSize), read Key, skip value, do until end of file
    // TODO: Consider efficienty, recursion vs. for loop or equivalent, file openings/buffers, etc
    // I wonder if there is a more "functional" way to interact with files/pointers.
    let rec fillDataMap (fileStore: FileStore) =
      let headerBuffer = HEADER_SIZE |> Array.zeroCreate<byte> // can this be moved above function def?
      fileStore.writeFileStream.ReadExactly headerBuffer
      let keySize = headerBuffer[0..3] |> BitConverter.ToInt32
      let valueSize = headerBuffer[4..7] |> BitConverter.ToInt32
      let keyBuffer = keySize |> Array.zeroCreate<byte>
      fileStore.writeFileStream.ReadExactly keyBuffer
      let key = keyBuffer |> bytesToString
      let writeSize = (HEADER_SIZE + keySize + valueSize)

      // set file position for the next read
      fileStore.writeFileStream.Seek (valueSize, SeekOrigin.Current) |> ignore

      let newFileStore = { 
        fileStore with 
          dataMap = 
            (match valueSize with // remove if there is a tombstone
            | 0 -> fileStore.dataMap |> Map.remove key;
            | _ -> fileStore.dataMap |> Map.add key (writeSize, fileStore.writePosition)); 
          writePosition = fileStore.writeFileStream.Position;
        }
      // printfn ("Length: %d  WritePositon: %d  WriteSize: %d") fileStore.writeFileStream.Length fileStore.writeFileStream.Position writeSize
      match fileStore.writeFileStream.Length <= fileStore.writeFileStream.Position with
      | true -> newFileStore
      | false -> fillDataMap newFileStore

    { 
      dataMap = Map.empty; 
      fileName = fileName; 
      writePosition = 0; 
      writeFileStream = File.Open (fileName, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
      readFileStream = File.Open (fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
    } |> fillDataMap
  
  //TODO: in theory your value could be any type - but only string for now
  let set (key: string) (value : string) (fileStore: FileStore) =
    // encode KV pair, write to file, update dataMap, update writePosition
    let (bytesLength, encodedBytes) = encodeKV 0 key (value |> System.Text.Encoding.ASCII.GetBytes)
    fileStore.writeFileStream.Write encodedBytes
    // fileStore.writeFileStream.Flush true; // TODO: will this be slow? -> yes
    let fileStore = { 
      fileStore with 
        dataMap = fileStore.dataMap |> Map.add key (bytesLength, fileStore.writePosition); 
        writePosition = fileStore.writeFileStream.Position;
      }
    fileStore

  // NOTE: this function assumes that everything has been flushed to file
  let readFromFile (fileStore : FileStore) (dataLength, dataPosition: int64) =
      fileStore.readFileStream.Seek (dataPosition,  SeekOrigin.Begin) |> ignore
      let buffer = dataLength |> Array.zeroCreate<byte>
      fileStore.readFileStream.ReadExactly buffer
      let (_, _, value) = buffer |> decodeKV
      value

  let get (key: string) (fileStore: FileStore) =
    // Find key in dataMap, open file, fetch data, decode data, return
    fileStore.writeFileStream.Flush true;
    fileStore.dataMap
    |> Map.tryFind key
    |> Option.map (readFromFile fileStore)
    

  let delete (key: string) (fileStore: FileStore) =
    let (_, encodedBytes) = encodeKV 0 key [||]
    fileStore.writeFileStream.Write encodedBytes
    fileStore.writeFileStream.Flush true; // TODO: will this be slow? -> yes
    let fileStore = { 
      fileStore with 
        dataMap = Map.remove key fileStore.dataMap;
        writePosition = fileStore.writeFileStream.Position; }
    fileStore

  let keys (fileStore: FileStore) =
    fileStore.dataMap |> Map.keys 

  let fold folder state (fileStore: FileStore) =
    fileStore.writeFileStream.Flush true;
    fileStore.dataMap 
    |> Map.fold (fun state key (dataLength, dataPosition: int64) ->
        (dataLength, dataPosition)
        |> readFromFile fileStore  
        |> folder state key) 
      state

  let print (fileStore: FileStore) =
    fileStore
    |> fold (fun _ key value -> printfn "Key: %s Value: %s" key value; ()) ()
    |> ignore

  type FileStoreOO(fileName, load) =
    let fileName = fileName

    let mutable dataMap = Map.empty // TODO: assigning to data map seems weird

    let mutable writeFileStream : FileStream = null

    let mutable readFileStream : FileStream = null

    let rec fillDataMap () =
      let startingWritePosition = writeFileStream.Position
      let headerBuffer = HEADER_SIZE |> Array.zeroCreate<byte> // can this be moved above function def?
      writeFileStream.ReadExactly headerBuffer
      let keySize = headerBuffer[0..3] |> BitConverter.ToInt32
      let valueSize = headerBuffer[4..7] |> BitConverter.ToInt32
      let keyBuffer = keySize |> Array.zeroCreate<byte>
      writeFileStream.ReadExactly keyBuffer
      let key = keyBuffer |> bytesToString
      let writeSize = (HEADER_SIZE + keySize + valueSize)

      // remove if there is a tombstone
      match valueSize with 
      | 0 ->  dataMap <- dataMap |> Map.remove key;
      | _ ->  dataMap <- dataMap |> Map.add key (writeSize, startingWritePosition); 
      
      writeFileStream.Seek (valueSize, SeekOrigin.Current) |> ignore
      match writeFileStream.Length <= writeFileStream.Position with
      | true -> () 
      | false -> fillDataMap ()

    let readFromFile (dataLength, dataPosition: int64) =
        let buffer = dataLength |> Array.zeroCreate<byte>
        readFileStream.Seek (dataPosition,  SeekOrigin.Begin) |> ignore
        readFileStream.ReadExactly buffer
        let (_, _, value) = buffer |> decodeKV
        value

    do 
      if load then
        writeFileStream <- File.Open (fileName, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
        readFileStream <- File.Open (fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        fillDataMap ()
      else
        writeFileStream <- File.Open (fileName, FileMode.Create, FileAccess.ReadWrite, FileShare.Read)
        readFileStream <- File.Open (fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
    
    member this.Set (key: string) (value : string) =
      // encode KV pair, write to file, update dataMap
      let (bytesLength, encodedBytes) = encodeKV 0 key (value |> System.Text.Encoding.ASCII.GetBytes)
      dataMap <- dataMap |> Map.add key (bytesLength, writeFileStream.Position);
      writeFileStream.Write encodedBytes
      // writeFileStream.Flush true; // TODO: will this be slow?
    
    member this.Get (key: string) =
      writeFileStream.Flush true;

      dataMap
      |> Map.tryFind key
      |> Option.map readFromFile

    member this.Delete (key: string) = 
      let (_, encodedBytes) = encodeKV 0 key [||]
      writeFileStream.Write encodedBytes
      writeFileStream.Flush true;
      dataMap <- dataMap |> Map.remove key

    member this.Keys  = dataMap |> Map.keys

    member this.Fold folder state = 
      writeFileStream.Flush true;
      dataMap |> Map.fold (fun state key (dataLength, dataPosition: int64) -> readFromFile (dataLength, dataPosition) |> folder state key) state

    member this.Print = this.Fold (fun _ key value -> printfn "Key: %s Value: %s" key value; ()) ()  |> ignore

    // TODO: is there a destructor I can use?
    member this.Close =
      if writeFileStream.CanWrite then
        writeFileStream.Flush true
      writeFileStream.Close ()
      readFileStream.Close ()