module BabyDbServer.DataService

open Microsoft.AspNetCore.Http
open Giraffe
open CaskStoreOO


[<CLIMutable>]
type KeyValue = {
    key: string
    value: string
}

type Value = {
    value : string
}

let getValueHandler (caskStore: CaskStoreOO) (key: string) =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {         
            match caskStore.Get key with 
            | Some value -> return! json ({ value = value}) next ctx
            | None -> 
                ctx.SetStatusCode 404
                return Some ctx
        }

let setValueHandler (caskStore: CaskStoreOO) =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            printfn "In setValueHandler"
            let! newKeyValue = ctx.BindJsonAsync<KeyValue>()
            // TODO: I don't think this will quite work given the functional implementation
            caskStore.Set newKeyValue.key newKeyValue.value |> ignore
            return! json (newKeyValue) next ctx
        }

let getAllValuesHandler (caskStore: CaskStoreOO) =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            // TODO: there might be a better way to do this
            let allKeyValue =
                caskStore.Fold (fun acc key value -> ({ key = key; value = value})::acc) []
            return! json (allKeyValue) next ctx
        }

let deleteValueHandler (caskStore: CaskStoreOO) (key: string) =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {         
            caskStore.Delete key
            ctx.SetStatusCode 204
            return! next ctx
        }