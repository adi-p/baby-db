module BabyDb.CaskStoreOO.Tests

open NUnit.Framework
open CaskStoreOO

[<TestFixture>]
type TestClass () =

  let folderName = "testCaskOO"

  let mutable (caskStoreOO : CaskStoreOO) = new CaskStoreOO(folderName, false) // TODO: need to find a better way to initialising this

  [<SetUp>]
  member this.SetUpCaskStoreOO() = 
    caskStoreOO.Close() // TODO: this is silly
    caskStoreOO <- new CaskStoreOO(folderName, false)

  [<TearDown>]
  member this.CloseCaskStoreOO() =
    caskStoreOO.Close()


  [<Test>]
  member this.TestGetNull() =
    let key = "key"
    Assert.That(caskStoreOO.Get key, Is.EqualTo(None))

  [<Test>]
  member this.TestGetNullAfterSet() =
    let key1 = "key"
    let (key2, value2) = ("key2", "value2")
    caskStoreOO.Set key2 value2
    Assert.That(caskStoreOO.Get key1, Is.EqualTo(None))

  [<Test>]
  member this.TestSetThenGetSingle() =
    let (key, value) = ("key", "value") 
    caskStoreOO.Set key value
    Assert.That(caskStoreOO.Get key, Is.EqualTo(Some value))
  
  [<Test>]
  member this.TestSetThenGetRepeateSingle() =
    let (key, value) = ("key", "value") 
    caskStoreOO.Set key value
    caskStoreOO.Set key value
    Assert.That(caskStoreOO.Get key, Is.EqualTo(Some value))

  [<Test>]
  member this.TestSetThenGetMultiple() =
    let (key1, value1) = ("key", "value")
    let (key2, value2) = ("key2", "value2")
    let (key3, value3) = ("key3", "value3")
    caskStoreOO.Set key1 value1 
    caskStoreOO.Set key2 value2
    caskStoreOO.Set key3 value3
    Assert.That(caskStoreOO.Get key1, Is.EqualTo(Some value1))
    Assert.That(caskStoreOO.Get key2, Is.EqualTo(Some value2))
    Assert.That(caskStoreOO.Get key3, Is.EqualTo(Some value3))

  [<Test>]
  member this.TestDoubleSet() =
    let (key, value1, value2) = ("key", "value", "value2")
    caskStoreOO.Set key value1 
    caskStoreOO.Set key value2
    Assert.That(caskStoreOO.Get key, Is.EqualTo(Some value2))

  [<Test>]
  member this.TestDeleteSingle() =
    let (key, value) = ("key", "value")
    caskStoreOO.Set key value
    caskStoreOO.Delete key
    Assert.That(caskStoreOO.Get key, Is.EqualTo(None))
  
  [<Test>]
  member this.TestDeleteMulti() =
    let (key1, value1) = ("key1", "value1")
    let (key2, value2) = ("key2", "value2")
    caskStoreOO.Set key1 value1 
    caskStoreOO.Set key2 value2 
    caskStoreOO.Delete key1
    Assert.That(caskStoreOO.Get key1, Is.EqualTo(None))
    Assert.That(caskStoreOO.Get key2, Is.EqualTo(Some value2))

  [<Test>]
  member this.TestDoubleDelete() =
    let (key1, value1) = ("key1", "value1")
    let (key2, value2) = ("key2", "value2")
    caskStoreOO.Set key1 value1 
    caskStoreOO.Set key2 value2 
    caskStoreOO.Delete key1
    caskStoreOO.Delete key1
    Assert.That(caskStoreOO.Get key1, Is.EqualTo(None))
    Assert.That(caskStoreOO.Get key2, Is.EqualTo(Some value2))

  [<Test>]
  member this.TestSetDeleteSet() =
    let (key, value1, value2) = ("key1", "value1", "value2")
    caskStoreOO.Set key value1 
    caskStoreOO.Delete key
    caskStoreOO.Set key value2
    Assert.That(caskStoreOO.Get key, Is.EqualTo(Some value2))


  // *** Load ***
  [<Test>]
  member this.TestLoadPersisted() =
    let (key1, value1) = ("key1", "value1")
    let (key2, value2) = ("key2", "value2")
    let (key3, value3) = ("key3", "value3")
    let (key4, value4) = ("key4", "value4")
    caskStoreOO.Set key1 value1
    caskStoreOO.Set key2 value2
    caskStoreOO.Set key3 value3
    caskStoreOO.Set key4 value4


    Assert.That(caskStoreOO.Get key1, Is.EqualTo(Some value1))
    Assert.That(caskStoreOO.Get key2, Is.EqualTo(Some value2))
    Assert.That(caskStoreOO.Get key3, Is.EqualTo(Some value3))
    Assert.That(caskStoreOO.Get key4, Is.EqualTo(Some value4))
    caskStoreOO.Close()

    caskStoreOO <- new CaskStoreOO(folderName, true)
    Assert.That(caskStoreOO.Get key1, Is.EqualTo(Some value1))
    Assert.That(caskStoreOO.Get key2, Is.EqualTo(Some value2))
    Assert.That(caskStoreOO.Get key3, Is.EqualTo(Some value3))
    Assert.That(caskStoreOO.Get key4, Is.EqualTo(Some value4))

  [<Test>]
  member this.TestDeleteLoadPersisted() =
    let (key1, value1) = ("key1", "value1")
    let (key2, value2) = ("key2", "value2")
    caskStoreOO.Set key1 value1
    caskStoreOO.Set key2 value2
    caskStoreOO.Delete key2


    Assert.That(caskStoreOO.Get key1, Is.EqualTo(Some value1))
    Assert.That(caskStoreOO.Get key2, Is.EqualTo(None))
    caskStoreOO.Close()

    caskStoreOO <- new CaskStoreOO(folderName, true)
    Assert.That(caskStoreOO.Get key1, Is.EqualTo(Some value1))
    Assert.That(caskStoreOO.Get key2, Is.EqualTo(None))

  [<Test>]
  member this.TestDeleteAfterLoad() =
    let (key1, value1) = ("key1", "value1")
    let (key2, value2) = ("key2", "value2")
    caskStoreOO.Set key1 value1
    caskStoreOO.Set key2 value2


    Assert.That(caskStoreOO.Get key1, Is.EqualTo(Some value1))
    caskStoreOO.Close()

    caskStoreOO <- new CaskStoreOO(folderName, true)
    caskStoreOO.Delete key2
    Assert.That(caskStoreOO.Get key1, Is.EqualTo(Some value1))
    Assert.That(caskStoreOO.Get key2, Is.EqualTo(None))

  [<Test>]
  member this.TestLoadSetPersisted() =
    let (key1, value1) = ("key1", "value1")
    let (key2, value2) = ("key2", "value2")
    caskStoreOO.Set key1 value1
    caskStoreOO.Set key2 value2


    Assert.That(caskStoreOO.Get key1, Is.EqualTo(Some value1))
    Assert.That(caskStoreOO.Get key2, Is.EqualTo(Some value2))
    caskStoreOO.Close()

    caskStoreOO <- new CaskStoreOO(folderName, true)
    let newValue = "newValue"
    Assert.That(caskStoreOO.Get key1, Is.EqualTo(Some value1))
    Assert.That(caskStoreOO.Get key2, Is.EqualTo(Some value2))

    caskStoreOO.Set key2 newValue
    Assert.That(caskStoreOO.Get key2, Is.EqualTo(Some newValue))
    caskStoreOO.Close()

    caskStoreOO <- new CaskStoreOO(folderName, true)
    Assert.That(caskStoreOO.Get key1, Is.EqualTo(Some value1))
    Assert.That(caskStoreOO.Get key2, Is.EqualTo(Some newValue))

  [<Test>]
  member this.TestLoadSetPersistedOverMultipleFileStores() =
    let (key1, value1) = ("key1", "value1")
    let (key2, value2, value2b) = ("key2", "value2", "value2b")
    let (key3, value3, value3b, value3c) = ("key3", "value3", "value3b", "value3c")
    caskStoreOO.Set key1 value1
    caskStoreOO.Set key2 value2
    caskStoreOO.Set key3 value3


    Assert.That(caskStoreOO.Get key1, Is.EqualTo(Some value1))
    Assert.That(caskStoreOO.Get key2, Is.EqualTo(Some value2))
    Assert.That(caskStoreOO.Get key3, Is.EqualTo(Some value3))
    caskStoreOO.Close()

    caskStoreOO <- new CaskStoreOO(folderName, true)
    Assert.That(caskStoreOO.Get key1, Is.EqualTo(Some value1))
    Assert.That(caskStoreOO.Get key2, Is.EqualTo(Some value2))

    caskStoreOO.Set key2 value2b
    caskStoreOO.Set key3 value3b
    Assert.That(caskStoreOO.Get key1, Is.EqualTo(Some value1))
    Assert.That(caskStoreOO.Get key2, Is.EqualTo(Some value2b))
    Assert.That(caskStoreOO.Get key3, Is.EqualTo(Some value3b))
    caskStoreOO.Close()


    caskStoreOO <- new CaskStoreOO(folderName, true)
    caskStoreOO.Set key3 value3c

    Assert.That(caskStoreOO.Get key1, Is.EqualTo(Some value1))
    Assert.That(caskStoreOO.Get key2, Is.EqualTo(Some value2b))
    Assert.That(caskStoreOO.Get key3, Is.EqualTo(Some value3c))
  

