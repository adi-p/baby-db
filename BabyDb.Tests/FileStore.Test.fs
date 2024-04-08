module BabyDb.FileStore.Tests

open System
open NUnit.Framework
open FileStore

[<TestFixture>]
type TestClass () =

  let file = "testDB"

  let mutable (fileStore : FileStore) = empty file // TODO: need to find a better way to initialising this

  [<SetUp>]
  member this.SetUpFileStore() = 
    close fileStore // TODO: this is silly
    fileStore <- empty file

  [<TearDown>]
  member this.CloseFileStore() =
    close fileStore

  [<Test>]
  member this.TestGetNull() =
    let key = "key"
    Assert.That(fileStore |> get key, Is.EqualTo(None))

  [<Test>]
  member this.TestGetNullAfterSet() =
    let key1 = "key"
    let (key2, value2) = ("key2", "value2")
    let fileStore = fileStore |> set key2 value2
    Assert.That(fileStore |> get key1, Is.EqualTo(None))

  [<Test>]
  member this.TestSetThenGetSingle() =
    let (key, value) = ("key", "value") 
    let fileStore = fileStore |> set key value
    Assert.That(fileStore |> get key, Is.EqualTo(Some value))
  
  [<Test>]
  member this.TestSetThenGetRepeateSingle() =
    let (key, value) = ("key", "value") 
    let fileStore = fileStore |> set key value |> set key value
    Assert.That(fileStore |> get key, Is.EqualTo(Some value))

  [<Test>]
  member this.TestSetThenGetMultiple() =
    let (key1, value1) = ("key", "value")
    let (key2, value2) = ("key2", "value2")
    let (key3, value3) = ("key3", "value3")
    let fileStore = 
      fileStore
      |> set key1 value1 
      |> set key2 value2
      |> set key3 value3
    Assert.That(fileStore |> get key1, Is.EqualTo(Some value1))
    Assert.That(fileStore |> get key2, Is.EqualTo(Some value2))
    Assert.That(fileStore |> get key3, Is.EqualTo(Some value3))

  [<Test>]
  member this.TestDoubleSet() =
    let (key, value1, value2) = ("key", "value", "value2")
    let fileStore = 
      fileStore 
      |> set key value1 
      |> set key value2
    Assert.That(fileStore |> get key, Is.EqualTo(Some value2))

  [<Test>]
  member this.TestDeleteSingle() =
    let (key, value) = ("key", "value")
    let fileStore = 
      fileStore 
      |> set key value
      |> delete key
    Assert.That(fileStore |> get key, Is.EqualTo(None))
  
  [<Test>]
  member this.TestDeleteMulti() =
    let (key1, value1) = ("key1", "value1")
    let (key2, value2) = ("key2", "value2")
    let fileStore = 
      fileStore 
      |> set key1 value1 
      |> set key2 value2 
      |> delete key1
    Assert.That(fileStore |> get key1, Is.EqualTo(None))
    Assert.That(fileStore |> get key2, Is.EqualTo(Some value2))

  [<Test>]
  member this.TestDoubleDelete() =
    let (key1, value1) = ("key1", "value1")
    let (key2, value2) = ("key2", "value2")
    let fileStore = 
      fileStore 
      |> set key1 value1 
      |> set key2 value2 
      |> delete key1
      |> delete key1
    Assert.That(fileStore |> get key1, Is.EqualTo(None))
    Assert.That(fileStore |> get key2, Is.EqualTo(Some value2))

  [<Test>]
  member this.TestSetDeleteSet() =
    let (key, value1, value2) = ("key1", "value1", "value2")
    let fileStore = 
      fileStore 
      |> set key value1 
      |> delete key
      |> set key value2
    Assert.That(fileStore |> get key, Is.EqualTo(Some value2))

  // *** Load ***
  [<Test>]
  member this.TestLoadPersisted() =
    let (key1, value1) = ("key1", "value1")
    let (key2, value2) = ("key2", "value2")
    let (key3, value3) = ("key3", "value3")
    let (key4, value4) = ("key4", "value4")
    fileStore <-
      fileStore
      |> set key1 value1
      |> set key2 value2
      |> set key3 value3
      |> set key4 value4


    Assert.That(fileStore |> get key1, Is.EqualTo(Some value1))
    Assert.That(fileStore |> get key2, Is.EqualTo(Some value2))
    Assert.That(fileStore |> get key3, Is.EqualTo(Some value3))
    Assert.That(fileStore |> get key4, Is.EqualTo(Some value4))
    close fileStore

    fileStore <- load file
    Assert.That(fileStore |> get key1, Is.EqualTo(Some value1))
    Assert.That(fileStore |> get key2, Is.EqualTo(Some value2))
    Assert.That(fileStore |> get key3, Is.EqualTo(Some value3))
    Assert.That(fileStore |> get key4, Is.EqualTo(Some value4))

  [<Test>]
  member this.TestDeleteLoadPersisted() =
    let (key1, value1) = ("key1", "value1")
    let (key2, value2) = ("key2", "value2")
    fileStore <-
      fileStore
      |> set key1 value1
      |> set key2 value2
      |> delete key2


    Assert.That(fileStore |> get key1, Is.EqualTo(Some value1))
    Assert.That(fileStore |> get key2, Is.EqualTo(None))
    close fileStore

    fileStore <- load file
    Assert.That(fileStore |> get key1, Is.EqualTo(Some value1))
    Assert.That(fileStore |> get key2, Is.EqualTo(None))

  [<Test>]
  member this.TestLoadSetPersisted() =
    let (key1, value1) = ("key1", "value1")
    let (key2, value2) = ("key2", "value2")
    fileStore <-
      fileStore
      |> set key1 value1
      |> set key2 value2


    Assert.That(fileStore |> get key1, Is.EqualTo(Some value1))
    Assert.That(fileStore |> get key2, Is.EqualTo(Some value2))
    close fileStore

    fileStore <- load file
    let newValue = "newValue"
    Assert.That(fileStore |> get key1, Is.EqualTo(Some value1))
    Assert.That(fileStore |> get key2, Is.EqualTo(Some value2))

    fileStore <- fileStore |> set key2 newValue
    Assert.That(fileStore |> get key2, Is.EqualTo(Some newValue))
    close fileStore

    fileStore <- load file
    Assert.That(fileStore |> get key1, Is.EqualTo(Some value1))
    Assert.That(fileStore |> get key2, Is.EqualTo(Some newValue))

