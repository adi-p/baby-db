module FileStoreOO
  open System
  open System.IO
  open Encode
  
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