module Encode
  open System

  // TODO: figure out if naming is correct and if more functionality should be moved here

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