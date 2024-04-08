module BabyDb.FileStoreOO.Tests

open System
open NUnit.Framework
open FileStore

[<TestFixture>]
type TestClass () =

  let mutable (fileStore : FileStoreOO) = new FileStoreOO("testDB", false)

  [<SetUp>]
  member this.SetUpFileStore() = 
    fileStore.Close // TODO: This feels kinda silly
    fileStore <- new FileStoreOO("testDB", false)

  [<TearDown>]
  member this.CloseFileStore() =
    fileStore.Close


  [<Test>]
  member this.TestGetNull() =
    let key = "key"
    Assert.That(fileStore.Get key, Is.EqualTo(None))

  [<Test>]
  member this.TestGetNullAfterSet() =
    let key1 = "key"
    let (key2, value2) = ("key2", "value2")
    fileStore.Set key2 value2
    Assert.That(fileStore.Get key1, Is.EqualTo(None))

  [<Test>]
  member this.TestSetThenGetSingle() =
    let (key, value) = ("key", "value") 
    fileStore.Set key value
    Assert.That(fileStore.Get key, Is.EqualTo(Some value))
  
  [<Test>]
  member this.TestSetThenGetRepeateSingle() =
    let (key, value) = ("key", "value") 
    fileStore.Set key value
    fileStore.Set key value
    Assert.That(fileStore.Get key, Is.EqualTo(Some value))

  [<Test>]
  member this.TestSetThenGetMultiple() =
    let (key1, value1) = ("key", "value")
    let (key2, value2) = ("key2", "value2")
    let (key3, value3) = ("key3", "value3")
    fileStore.Set key1 value1 
    fileStore.Set key2 value2
    fileStore.Set key3 value3
    Assert.That(fileStore.Get key1, Is.EqualTo(Some value1))
    Assert.That(fileStore.Get key2, Is.EqualTo(Some value2))
    Assert.That(fileStore.Get key3, Is.EqualTo(Some value3))

  [<Test>]
  member this.TestDoubleSet() =
    let (key, value1, value2) = ("key", "value", "value2")
    fileStore.Set key value1 
    fileStore.Set key value2
    Assert.That(fileStore.Get key, Is.EqualTo(Some value2))

  [<Test>]
  member this.TestDeleteSingle() =
    let (key, value) = ("key", "value")
    fileStore.Set key value
    fileStore.Delete key
    Assert.That(fileStore.Get key, Is.EqualTo(None))
  
  [<Test>]
  member this.TestDeleteMulti() =
    let (key1, value1) = ("key1", "value1")
    let (key2, value2) = ("key2", "value2")
    fileStore.Set key1 value1 
    fileStore.Set key2 value2 
    fileStore.Delete key1
    Assert.That(fileStore.Get key1, Is.EqualTo(None))
    Assert.That(fileStore.Get key2, Is.EqualTo(Some value2))

  [<Test>]
  member this.TestDoubleDelete() =
    let (key1, value1) = ("key1", "value1")
    let (key2, value2) = ("key2", "value2")
    fileStore.Set key1 value1 
    fileStore.Set key2 value2 
    fileStore.Delete key1
    fileStore.Delete key1
    Assert.That(fileStore.Get key1, Is.EqualTo(None))
    Assert.That(fileStore.Get key2, Is.EqualTo(Some value2))

  [<Test>]
  member this.TestSetDeleteSet() =
    let (key, value1, value2) = ("key1", "value1", "value2")
    fileStore.Set key value1 
    fileStore.Delete key
    fileStore.Set key value2
    Assert.That(fileStore.Get key, Is.EqualTo(Some value2))

// TODO: test load filestore
