module MemStore
    
    type MemStore = {
        dataMap : Map<string, string>
    }

    let empty = 
        { dataMap = Map.empty }
    
    let set (memstore: MemStore) (key: string) (value : string) = 
        { dataMap = Map.add key value memstore.dataMap }

    let get (memstore: MemStore) (key: string) = 
        Map.tryFind key memstore.dataMap

    let delete (memstore: MemStore) (key: string) =
        { dataMap = Map.remove key memstore.dataMap}