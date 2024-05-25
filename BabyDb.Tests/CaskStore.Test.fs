module BabyDb.CaskStore.Tests

open NUnit.Framework
open CaskStore

[<TestFixture>]
type TestClass () =

  let folderName = "testCask"

  let mutable (caskStore : CaskStore) = empty folderName // TODO: need to find a better way to initialising this

  [<SetUp>]
  member this.SetUpCaskStore() = 
    close caskStore // TODO: this is silly
    caskStore <- empty folderName

  [<TearDown>]
  member this.CloseCaskStore() =
    close caskStore

  [<Test>]
  member this.TestGetNull() =
    let key = "key"
    Assert.That(caskStore |> get key, Is.EqualTo(None))

  [<Test>]
  member this.TestGetNullAfterSet() =
    let key1 = "key"
    let (key2, value2) = ("key2", "value2")
    let caskStore = caskStore |> set key2 value2
    Assert.That(caskStore |> get key1, Is.EqualTo(None))

  [<Test>]
  member this.TestSetThenGetSingle() =
    let (key, value) = ("key", "value") 
    let caskStore = caskStore |> set key value
    Assert.That(caskStore |> get key, Is.EqualTo(Some value))
  
  [<Test>]
  member this.TestSetThenGetRepeateSingle() =
    let (key, value) = ("key", "value") 
    let caskStore = caskStore |> set key value |> set key value
    Assert.That(caskStore |> get key, Is.EqualTo(Some value))

  [<Test>]
  member this.TestSetThenGetMultiple() =
    let (key1, value1) = ("key", "value")
    let (key2, value2) = ("key2", "value2")
    let (key3, value3) = ("key3", "value3")
    let caskStore = 
      caskStore
      |> set key1 value1 
      |> set key2 value2
      |> set key3 value3
    Assert.That(caskStore |> get key1, Is.EqualTo(Some value1))
    Assert.That(caskStore |> get key2, Is.EqualTo(Some value2))
    Assert.That(caskStore |> get key3, Is.EqualTo(Some value3))

  [<Test>]
  member this.TestDoubleSet() =
    let (key, value1, value2) = ("key", "value", "value2")
    let caskStore = 
      caskStore 
      |> set key value1 
      |> set key value2
    Assert.That(caskStore |> get key, Is.EqualTo(Some value2))

  [<Test>]
  member this.TestDeleteSingle() =
    let (key, value) = ("key", "value")
    let caskStore = 
      caskStore 
      |> set key value
      |> delete key
    Assert.That(caskStore |> get key, Is.EqualTo(None))
  
  [<Test>]
  member this.TestDeleteMulti() =
    let (key1, value1) = ("key1", "value1")
    let (key2, value2) = ("key2", "value2")
    let caskStore = 
      caskStore 
      |> set key1 value1 
      |> set key2 value2 
      |> delete key1
    Assert.That(caskStore |> get key1, Is.EqualTo(None))
    Assert.That(caskStore |> get key2, Is.EqualTo(Some value2))

  [<Test>]
  member this.TestDoubleDelete() =
    let (key1, value1) = ("key1", "value1")
    let (key2, value2) = ("key2", "value2")
    let caskStore = 
      caskStore 
      |> set key1 value1 
      |> set key2 value2 
      |> delete key1
      |> delete key1
    Assert.That(caskStore |> get key1, Is.EqualTo(None))
    Assert.That(caskStore |> get key2, Is.EqualTo(Some value2))

  [<Test>]
  member this.TestSetDeleteSet() =
    let (key, value1, value2) = ("key1", "value1", "value2")
    let caskStore = 
      caskStore 
      |> set key value1 
      |> delete key
      |> set key value2
    Assert.That(caskStore |> get key, Is.EqualTo(Some value2))


  // *** Load ***
  [<Test>]
  member this.TestLoadPersisted() =
    let (key1, value1) = ("key1", "value1")
    let (key2, value2) = ("key2", "value2")
    let (key3, value3) = ("key3", "value3")
    let (key4, value4) = ("key4", "value4")
    caskStore <-
      caskStore
      |> set key1 value1
      |> set key2 value2
      |> set key3 value3
      |> set key4 value4


    Assert.That(caskStore |> get key1, Is.EqualTo(Some value1))
    Assert.That(caskStore |> get key2, Is.EqualTo(Some value2))
    Assert.That(caskStore |> get key3, Is.EqualTo(Some value3))
    Assert.That(caskStore |> get key4, Is.EqualTo(Some value4))
    close caskStore

    caskStore <- load folderName
    Assert.That(caskStore |> get key1, Is.EqualTo(Some value1))
    Assert.That(caskStore |> get key2, Is.EqualTo(Some value2))
    Assert.That(caskStore |> get key3, Is.EqualTo(Some value3))
    Assert.That(caskStore |> get key4, Is.EqualTo(Some value4))

  [<Test>]
  member this.TestDeleteLoadPersisted() =
    let (key1, value1) = ("key1", "value1")
    let (key2, value2) = ("key2", "value2")
    caskStore <-
      caskStore
      |> set key1 value1
      |> set key2 value2
      |> delete key2


    Assert.That(caskStore |> get key1, Is.EqualTo(Some value1))
    Assert.That(caskStore |> get key2, Is.EqualTo(None))
    close caskStore

    caskStore <- load folderName
    Assert.That(caskStore |> get key1, Is.EqualTo(Some value1))
    Assert.That(caskStore |> get key2, Is.EqualTo(None))

  [<Test>]
  member this.TestDeleteAfterLoad() =
    let (key1, value1) = ("key1", "value1")
    let (key2, value2) = ("key2", "value2")
    caskStore <-
      caskStore
      |> set key1 value1
      |> set key2 value2


    Assert.That(caskStore |> get key1, Is.EqualTo(Some value1))
    close caskStore

    caskStore <- load folderName |> delete key2
    Assert.That(caskStore |> get key1, Is.EqualTo(Some value1))
    Assert.That(caskStore |> get key2, Is.EqualTo(None))

  [<Test>]
  member this.TestLoadSetPersisted() =
    let (key1, value1) = ("key1", "value1")
    let (key2, value2) = ("key2", "value2")
    caskStore <-
      caskStore
      |> set key1 value1
      |> set key2 value2


    Assert.That(caskStore |> get key1, Is.EqualTo(Some value1))
    Assert.That(caskStore |> get key2, Is.EqualTo(Some value2))
    close caskStore

    caskStore <- load folderName
    let newValue = "newValue"
    Assert.That(caskStore |> get key1, Is.EqualTo(Some value1))
    Assert.That(caskStore |> get key2, Is.EqualTo(Some value2))

    caskStore <- caskStore |> set key2 newValue
    Assert.That(caskStore |> get key2, Is.EqualTo(Some newValue))
    close caskStore

    caskStore <- load folderName
    Assert.That(caskStore |> get key1, Is.EqualTo(Some value1))
    Assert.That(caskStore |> get key2, Is.EqualTo(Some newValue))

  [<Test>]
  member this.TestLoadSetPersistedOverMultipleFileStores() =
    let (key1, value1) = ("key1", "value1")
    let (key2, value2, value2b) = ("key2", "value2", "value2b")
    let (key3, value3, value3b, value3c) = ("key3", "value3", "value3b", "value3c")
    caskStore <-
      caskStore
      |> set key1 value1
      |> set key2 value2
      |> set key3 value3


    Assert.That(caskStore |> get key1, Is.EqualTo(Some value1))
    Assert.That(caskStore |> get key2, Is.EqualTo(Some value2))
    Assert.That(caskStore |> get key3, Is.EqualTo(Some value3))
    close caskStore

    caskStore <- load folderName
    Assert.That(caskStore |> get key1, Is.EqualTo(Some value1))
    Assert.That(caskStore |> get key2, Is.EqualTo(Some value2))

    caskStore <- caskStore |> set key2 value2b |> set key3 value3b
    Assert.That(caskStore |> get key1, Is.EqualTo(Some value1))
    Assert.That(caskStore |> get key2, Is.EqualTo(Some value2b))
    Assert.That(caskStore |> get key3, Is.EqualTo(Some value3b))
    close caskStore

    caskStore <- load folderName
    caskStore <- caskStore |> set key3 value3c

    Assert.That(caskStore |> get key1, Is.EqualTo(Some value1))
    Assert.That(caskStore |> get key2, Is.EqualTo(Some value2b))
    Assert.That(caskStore |> get key3, Is.EqualTo(Some value3c))
  

