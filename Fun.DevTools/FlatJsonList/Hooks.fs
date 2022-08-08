[<AutoOpen>]
module Fun.DevTools.FlatJsonList.Hooks

open System
open System.Timers
open System.Text
open System.Text.Json
open System.Collections.Generic
open System.Diagnostics
open FSharp.Data.Adaptive
open Microsoft.JSInterop
open MudBlazor
open Blazored.LocalStorage
open Fun.Blazor
open Fun.DevTools


type State =
    {
        Spliter: string
        Keys: string list
        Jsons: Map<string, Dictionary<string, string>>
        BaseJsonName: string option
    }

    static member DefaultValue = {
        Spliter = "."
        Keys = []
        Jsons = Map.empty
        BaseJsonName = None
    }


let private keyprefix = "FlatJsonList"

let private cachedStateKey = keyprefix + "-State"


type IComponentHook with

    member hook.State = hook.ShareStore.CreateCVal(keyprefix + nameof hook.State, State.DefaultValue)

    member hook.KeysFilter = hook.ShareStore.CreateCVal(keyprefix + nameof hook.KeysFilter, "")

    member hook.KeysSortIsASC = hook.ShareStore.CreateCVal(keyprefix + nameof hook.KeysSortIsASC, true)


    member hook.InitFlatJsonList() =
        let storage = hook.ServiceProvider.GetMultipleServices<ILocalStorageService>()

        hook.AddFirstAfterRenderTask(fun _ -> task {
            try
                let! data = storage.GetItemAsync<State>(cachedStateKey)
                if data |> box |> isNull |> not then hook.State.Publish data
            with ex ->
                printfn "Load cache failed for FlatJsonlist: %s" ex.Message

            let timer = new Timer(Interval = 30_000)
            hook.AddDispose timer
            timer.Elapsed.Add(fun _ ->
                task {
                    do! storage.SetItemAsync(cachedStateKey, hook.State.Value)
                    printfn "Cache state for FlatJsonList"
                }
                |> ignore
            )
            timer.Start()
        }
        )


    member hook.FilteredKeys = adaptive {
        let! keys = hook.State |> AVal.map (fun x -> x.Keys)
        let! filter = hook.KeysFilter
        let! isAsc = hook.KeysSortIsASC
        return
            if String.IsNullOrEmpty filter then
                keys :> string seq
            else
                keys |> Seq.filter (fun x -> x.Contains(filter, StringComparison.OrdinalIgnoreCase))
            |> (if isAsc then Seq.sort else Seq.sortDescending)
            |> Seq.toList
    }


    member hook.ClearAll() =
        let storage = hook.ServiceProvider.GetMultipleServices<ILocalStorageService>()

        storage.RemoveItemAsync cachedStateKey |> ignore

        transact (fun _ ->
            hook.State.Value <- State.DefaultValue
            hook.KeysFilter.Value <- ""
        )


    member hook.ExportAll() = task {
        let js, snack, storage = hook.ServiceProvider.GetMultipleServices<IJSRuntime * ISnackbar * ILocalStorageService>()
        try
            let sw = Stopwatch.StartNew()
            let state = hook.State.Value

            for KeyValue (fileName, parsedFile) in state.Jsons do
                let jsonStr =
                    parsedFile
                    |> Seq.map (fun (KeyValue (k, v)) -> k, v)
                    |> Seq.toList
                    |> createJsonFromFlatList state.Spliter ""
                    |> fun x -> JsonSerializer.Serialize(x, jsonSerializeOptions)
                do! js.saveFile (fileName, jsonStr)

            do! storage.RemoveItemAsync cachedStateKey

            snack.Add($"Export all flat json spent {sw.ElapsedMilliseconds}ms") |> ignore

        with ex ->
            snack.Add(ex.Message, Severity.Error) |> ignore
    }


    member hook.DeleteFlatJson jsonName = hook.State.Publish(fun state -> { state with Jsons = state.Jsons |> Map.remove jsonName })


    member hook.AddFlatJson(jsonName, data: byte[]) =
        let snack = hook.ServiceProvider.GetMultipleServices<ISnackbar>()
        try
            let sw = Stopwatch.StartNew()

            hook.State.Publish(fun state ->
                let jsonStr = Encoding.UTF8.GetString data
                let flattedJson = fromJsonToDict state.Spliter jsonStr
                let isFirstJson = state.Jsons.Count = 0
                let jsons = state.Jsons |> Map.add jsonName flattedJson

                { state with
                    Jsons = jsons
                    BaseJsonName = if isFirstJson then Some jsonName else state.BaseJsonName
                    Keys = if isFirstJson then jsons[jsonName].Keys |> Seq.toList else state.Keys
                }
            )

            snack.Add($"Add flat json spent {sw.ElapsedMilliseconds}ms") |> ignore

        with ex ->
            snack.Add(string ex, Severity.Error) |> ignore
