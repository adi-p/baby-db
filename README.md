# Baby-db

Baby-db is a key-value store backed by a log structured hash table and persisted to disk.

It is based on [CaskDB](https://github.com/avinassh/py-caskdb/) and [Bitcask](https://riak.com/assets/bitcask-intro.pdf)

Baby-db is written in F#. It is meant as an educational project and is not production ready

## Installation

Install .NET and F# following https://learn.microsoft.com/en-us/dotnet/fsharp/get-started/install-fsharp

## Folder Structure

- BabyDb
- BabyDb.Tests
- BabyDbServer

### BabyDb

This is the main piece of this project. It is the code for the key-value store.

To run, navigate to the `/BabyDB` folder and run:

`dotnet run`

### BabyDb.Tests

This folder contains the tests for baby-db.

To run the tests, navigate to the `/BabyDB.Tests` folder and run: 

`dotnet test`

### BabyDbServer

This folder contains a small web app that interacts with BabyDb.

It uses F# and Giraffe in the back end, and React in the front end.

To run the back end, navigate to `/BabyDbServer` and use `dotnet run` or `dotnet watch`

For the front end, navigate to `/BabyDbServer/client` and use `npm start`

## Basic Usage

```F#

// create a empty CaskStore called "test_db" and add a kv pair
let store =
  CaskStore.empty "test_db"
  |> CaskStore.set "key_1" "value_1"

// Try and get value and print
store
|> CaskStore.get "key_1"
|> Option.defaultValue "ERROR - Value not found"
|> printfn "%s" // will print 'value_1'

store
|> CaskStore.get "key_2"
|> Option.defaultValue "ERROR - Value not found"
|> printfn "%s" // will print 'ERROR - Value not found'

// delete key
let store = store |> CaskStore.delete "key_1"

store
|> CaskStore.get "key_1"
|> Option.defaultValue "ERROR - Value not found"
|> printfn "%s" // will print 'ERROR - Value not found'

```

Alternatively, using the OO version

```F#

open CaskStoreOO

let store = new CaskStoreOO("test_db", false)
store.Set "key_1" "value_1"

// Try and get value and print
store.Get "key_1"
|> Option.defaultValue "ERROR - Value not found"
|> printfn "%s" // will print 'value_1'

store.Get "key_2"
|> Option.defaultValue "ERROR - Value not found"
|> printfn "%s" // will print 'ERROR - Value not found'

// delete key
store.Delete "key_1"

store.Get "key_1"
|> Option.defaultValue "ERROR - Value not found"
|> printfn "%s" // will print 'ERROR - Value not found'

```
