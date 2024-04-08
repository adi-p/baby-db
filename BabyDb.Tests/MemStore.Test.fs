module BabyDb.MemStore.Tests

open System
open NUnit.Framework
open MemStore

[<TestFixture>]
type TestClass () =

  let mutable (memStore : MemStore) = empty

  [<SetUp>]
  member this.SetUpMemStore() =
    memStore <- empty

  [<Test>]
  member this.TestGetNull() =
    let key = "key"
    Assert.That(memStore |> get key, Is.EqualTo(None))

  [<Test>]
  member this.TestGetNullAfterSet() =
    let key1 = "key"
    let (key2, value2) = ("key2", "value2")
    let memStore = memStore |> set key2 value2
    Assert.That(memStore |> get key1, Is.EqualTo(None))

  [<Test>]
  member this.TestSetThenGetSingle() =
    let (key, value) = ("key", "value") 
    let memStore = memStore |> set key value
    Assert.That(memStore |> get key, Is.EqualTo(Some value))
  
  [<Test>]
  member this.TestSetThenGetRepeateSingle() =
    let (key, value) = ("key", "value") 
    let memStore = memStore |> set key value |> set key value
    Assert.That(memStore |> get key, Is.EqualTo(Some value))

  [<Test>]
  member this.TestSetThenGetMultiple() =
    let (key1, value1) = ("key", "value")
    let (key2, value2) = ("key2", "value2")
    let (key3, value3) = ("key3", "value3")
    let memStore = 
      memStore
      |> set key1 value1 
      |> set key2 value2
      |> set key3 value3
    Assert.That(memStore |> get key1, Is.EqualTo(Some value1))
    Assert.That(memStore |> get key2, Is.EqualTo(Some value2))
    Assert.That(memStore |> get key3, Is.EqualTo(Some value3))

  [<Test>]
  member this.TestReset() =
    let (key, value1, value2) = ("key", "value", "value2")
    let memStore = 
      memStore 
      |> set key value1 
      |> set key value2
    Assert.That(memStore |> get key, Is.EqualTo(Some value2))

  [<Test>]
  member this.TestDeleteSingle() =
    let (key, value) = ("key", "value")
    let memStore = 
      memStore 
      |> set key value
      |> delete key
    Assert.That(memStore |> get key, Is.EqualTo(None))
  
  [<Test>]
  member this.TestDeleteMulti() =
    let (key1, value1) = ("key1", "value1")
    let (key2, value2) = ("key2", "value2")
    let memStore = 
      memStore 
      |> set key1 value1 
      |> set key2 value2 
      |> delete key1
    Assert.That(memStore |> get key1, Is.EqualTo(None))
    Assert.That(memStore |> get key2, Is.EqualTo(Some value2))

  [<Test>]
  member this.TestSetDeleteSet() =
    let (key, value1, value2) = ("key1", "value1", "value2")
    let memStore = 
      memStore 
      |> set key value1 
      |> delete key
      |> set key value2
    Assert.That(memStore |> get key, Is.EqualTo(Some value2))
