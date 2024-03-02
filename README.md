# Baby-db

Baby-db is a key-value store backed by a log structured hash table and file presistence.

It is based on [CaskDB](https://github.com/avinassh/py-caskdb/) and [Bitcask](https://riak.com/assets/bitcask-intro.pdf)

Baby-db is written in F#. It is meant as an educational project and is not production ready

## Installation

Install .NET and F# following https://learn.microsoft.com/en-us/dotnet/fsharp/get-started/install-fsharp

## Running

`dotnet run`

## Basic Usage

```F#

// create a empty filestore called "test_db" and add a kv pair
let store =
  FileStore.empty "test_db"
  |> FileStore.set "key_1" "value_1"

// Try and get value and print
store
|> FileStore.get "key_1"
|> Option.defaultValue "ERROR - Value not found"
|> printfn "%s" // will print 'value_1'

store
|> FileStore.get "key_2"
|> Option.defaultValue "ERROR - Value not found"
|> printfn "%s" // will print 'ERROR - Value not found'

// delete key
let store = store |> FileStore.delete "key_1"

store
|> FileStore.get "key_1"
|> Option.defaultValue "ERROR - Value not found"
|> printfn "%s" // will print 'ERROR - Value not found'

```
