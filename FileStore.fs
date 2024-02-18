
module FileStore
  open System
  open System.IO

  // TODO::
  // Do something similar to FileStore but actually write to a file. Base it on https://github.com/avinassh/py-caskdb/
  // for writing to file look into streamwriter

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

  let encodeKV timestamp (key: string) (value: string) =
    // TODO: use timestamp!
    let keyBytes = key |> System.Text.Encoding.ASCII.GetBytes
    let valueBytes = value |> System.Text.Encoding.ASCII.GetBytes
    let keySize = keyBytes |> Array.length |> BitConverter.GetBytes 
    let valueSize = valueBytes |> Array.length |> BitConverter.GetBytes
    let byteArray = seq { keySize; valueSize; keyBytes; valueBytes; } |> Array.concat
    byteArray |> Array.length , byteArray
  
  let decodeKV (bytes: byte[]) =
    // Get key size, get value size, read key, read value, return
    // TODO: use timestamp
    let keyStartIndex = 8; 
    let keySize = bytes[0..3] |> BitConverter.ToInt32
    let valueSize = bytes[4..7] |> BitConverter.ToInt32
    let key = bytes[keyStartIndex..(keyStartIndex + keySize-1)] |> System.Text.Encoding.ASCII.GetString
    let value = bytes[(keyStartIndex + keySize)..(keyStartIndex + keySize+valueSize-1)] |> System.Text.Encoding.ASCII.GetString
    // printfn "Decoded Bytes %d %d %s %s " keySize valueSize key value
    // printfn "buffer size %d" (bytes |> Array.length) 
    
    (0, key, value)


  type FileStore = {
    fileName : string
    dataMap : Map<string, int*int> // a map from key to (byte length, byte offset in file)
    writePosition: int
  }

  let empty fileName = 
    // TODO: check if file already exists?
    let file = File.Create fileName
    file.Close ()
    { dataMap = Map.empty; fileName = fileName; writePosition = 0 }

  let load fileName =
    empty fileName
    // TODO: this should find the current file and create the dataMap and set the writePosition

  
  let set (key: string) (value : string) (fileStore: FileStore) =
    // open file, encode KV pair, write to file, close file, update dataMap, update writePosition
    let file = File.OpenWrite fileStore.fileName
    let (bytesLength, encodedBytes) = encodeKV 0 key value
    file.Seek (fileStore.writePosition,  SeekOrigin.Begin) |> ignore
    file.Write encodedBytes
    file.Close ()
    { 
      fileStore with 
        dataMap = fileStore.dataMap |> Map.add key (bytesLength, fileStore.writePosition); 
        writePosition = fileStore.writePosition + (encodedBytes |> Array.length);
      }




  let get (key: string) (fileStore: FileStore) =
    // Find key in dataMap, open file, fetch data, close file, decode data, return
    match Map.tryFind key fileStore.dataMap with
    | None -> None
    | Some (dataLength ,dataPosition) ->
      let file = File.OpenRead fileStore.fileName
      file.Seek (dataPosition,  SeekOrigin.Begin) |> ignore
      let buffer = dataLength |> Array.zeroCreate<byte>
      file.ReadExactly buffer
      file.Close ()
      let (_, _, value) = buffer |> decodeKV
      Some value 
    

  let delete (key: string) (fileStore: FileStore) = ()
    // { fileStore with dataMap = Map.remove key fileStore.dataMap}
    // TODO: idea -> remove from dataMap and add tombstone? -> would need this for FileStore.Load function