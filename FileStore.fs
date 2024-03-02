
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

  let HEADER_SIZE = sizeof<int32> * 2

  let (bytesToString : byte[] -> string) = System.Text.Encoding.ASCII.GetString

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
    let keySize = bytes[0..3] |> BitConverter.ToInt32
    let valueSize = bytes[4..7] |> BitConverter.ToInt32
    let key = bytes[HEADER_SIZE..(HEADER_SIZE + keySize-1)] |> bytesToString
    let value = bytes[(HEADER_SIZE + keySize)..(HEADER_SIZE + keySize+valueSize-1)] |> bytesToString
    (0, key, value)


  type FileStore = {
    fileName : string
    dataMap : Map<string, int*int64> // a map from key to (byte length, byte offset in file)
    writePosition: int64
  }

  let empty fileName = 
    // TODO: check if file already exists?
    let file = File.Create fileName
    file.Close ()
    { dataMap = Map.empty; fileName = fileName; writePosition = 0 }

  let load fileName =
    // Find the current file and create the dataMap and set the writePosition
    // Read first 8 bytes (keySize and valueSize), read Key, skip value, do until end of file
    // TODO: need to deal with delete
    // TODO: Consider efficienty, recursion vs. for loop or equivalent, file openings/buffers, etc
    // I wonder if there is a more "functional" way to interact with files/pointers.
    let rec fillDataMap (file: FileStream) (fileStore: FileStore) =
      let headerBuffer = HEADER_SIZE |> Array.zeroCreate<byte>
      file.ReadExactly headerBuffer
      let keySize = headerBuffer[0..3] |> BitConverter.ToInt32
      let valueSize = headerBuffer[4..7] |> BitConverter.ToInt32
      let keyBuffer = keySize |> Array.zeroCreate<byte>
      file.ReadExactly keyBuffer
      let key = keyBuffer |> bytesToString
      let writeSize = (HEADER_SIZE + keySize + valueSize)

      // set file position for the next read
      file.Seek (valueSize, SeekOrigin.Current) |> ignore

      let newFileStore = { 
        fileStore with 
          dataMap = fileStore.dataMap |> Map.add key (writeSize, fileStore.writePosition); 
          writePosition = file.Position  
        }
      // printfn ("Length: %d  WritePositon: %d  WriteSize: %d") file.Length file.Position writeSize
      match file.Length < file.Position + int64(writeSize) with
      | true -> 
        file.Close ()
        newFileStore
      | false -> fillDataMap file newFileStore

    let file = File.OpenRead fileName
    fillDataMap file { dataMap = Map.empty; fileName = fileName; writePosition = 0 }
  
  let set (key: string) (value : string) (fileStore: FileStore) =
    // open file, encode KV pair, write to file, close file, update dataMap, update writePosition
    let file = File.OpenWrite fileStore.fileName
    let (bytesLength, encodedBytes) = encodeKV 0 key value
    // TODO: this seek might be expensive. And since it's just seeking to the same place it was last opened
    // that might be unnecessary - maybe we can keep a filestream open for writting
    file.Seek (fileStore.writePosition,  SeekOrigin.Begin) |> ignore
    file.Write encodedBytes
    let fileStore = { 
      fileStore with 
        dataMap = fileStore.dataMap |> Map.add key (bytesLength, fileStore.writePosition); 
        writePosition = file.Position;
      }
    file.Close ()
    fileStore

  let get (key: string) (fileStore: FileStore) =
    // Find key in dataMap, open file, fetch data, close file, decode data, return
    let getValueFromFile (dataLength, dataPosition: int64) =
      let file = File.OpenRead fileStore.fileName
      file.Seek (dataPosition,  SeekOrigin.Begin) |> ignore
      let buffer = dataLength |> Array.zeroCreate<byte>
      file.ReadExactly buffer
      file.Close ()
      let (_, _, value) = buffer |> decodeKV
      Some value
    fileStore.dataMap
    |> Map.tryFind key
    |> Option.bind getValueFromFile
    

  let delete (key: string) (fileStore: FileStore) =
    { fileStore with dataMap = Map.remove key fileStore.dataMap}
    // TODO: idea -> remove from dataMap and add tombstone? -> would need this for FileStore.Load function
    // TODO: what would a tombstone look like? no value? Special value? flag? -> flag is interesting it terms of efficientcy with garbage collection