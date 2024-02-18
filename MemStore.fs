module MemStore
    
    type MemStore = {
        dataMap : Map<string, string>
    }

    let empty = 
        { dataMap = Map.empty }
    
    let set (key: string) (value : string) (memstore: MemStore) = 
        { dataMap = Map.add key value memstore.dataMap }

    let get (key: string) (memstore: MemStore) = 
        Map.tryFind key memstore.dataMap

    let delete (key: string) (memstore: MemStore) =
        { dataMap = Map.remove key memstore.dataMap}