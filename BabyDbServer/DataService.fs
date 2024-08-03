module BabyDbServer.DataService

open Microsoft.AspNetCore.Http
open Giraffe
open CaskStore


[<CLIMutable>]
type KeyValue = {
    key: string
    value: string
}

type Value = {
    value : string
}

let getValueHandler (caskStore: CaskStore) (key: string) =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {         
            match caskStore |> get key with 
            | Some value -> return! json ({ value = value}) next ctx
            | None -> 
                ctx.SetStatusCode 404
                return Some ctx
        }

let setValueHandler (caskStore: CaskStore) =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            printfn "In setValueHandler"
            let! newKeyValue = ctx.BindJsonAsync<KeyValue>()
            // TODO: I don't think this will quite work given the functional implementation
            caskStore |> set newKeyValue.key newKeyValue.value |> ignore
            return! json (newKeyValue) next ctx
        }

let getAllValuesHandler (caskStore: CaskStore) =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            // TODO: there might be a better way to do this
            let allKeyValue = 
                caskStore 
                |> fold (fun acc key value -> ({ key = key; value = value})::acc) []
            return! json (allKeyValue) next ctx
        }