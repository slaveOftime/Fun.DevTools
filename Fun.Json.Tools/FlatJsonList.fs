[<AutoOpen>]
module Fun.Json.Tools.FlatJsonList

open System
open System.Text
open System.Text.Json
open System.Text.Json.Nodes
open System.Collections
open FSharp.Data.Adaptive
open Microsoft.JSInterop
open Fun.Blazor
open MudBlazor
open Fun.Json.Tools.Controls


let rec private flatJson spliter (dict: Generic.IDictionary<string, string>) (token: JsonElement) (prefix: string) =
    let (<.>) prefix name = if String.IsNullOrEmpty(prefix) then name else prefix + spliter + name

    match token.ValueKind with
    | JsonValueKind.Object ->
        for prop in token.EnumerateObject() do
            flatJson spliter dict prop.Value (prefix <.> prop.Name)
    | JsonValueKind.Array ->
        let mutable index = 0
        for value in token.EnumerateArray() do
            flatJson spliter dict value (prefix <.> string index)
            index <- index + 1
    | _ -> dict.[prefix] <- token.GetString()


let private fromJsonToDict spliter (json: string) =
    let doc = JsonDocument.Parse(json)
    let dict = Generic.Dictionary()
    flatJson spliter dict doc.RootElement ""
    dict


let rec createJsonFromFlatList spliter (path: string) (parsedFile: List<string * string>) =
    let inline (</>) str1 str2 =
        if String.IsNullOrEmpty str1 then str2
        elif String.IsNullOrEmpty str2 then str1
        else str1 + spliter + str2

    let inline getKey (key: string) =
        let startIndex = if String.IsNullOrEmpty path then 0 else path.Length + 1
        let subKey = key.Substring startIndex
        let spliterIndex = subKey.IndexOf spliter
        if spliterIndex > -1 then
            subKey.Substring(0, subKey.IndexOf spliter)
        else
            subKey

    let inline splitKeyValues (subPath: string) (keyValues: (string * string) list) =
        keyValues
        |> List.partition (fun (key, _) -> key.Length <= subPath.Length || not (key.Substring(subPath.Length + 1).Contains(spliter)))

    parsedFile
    |> List.groupBy (fst >> getKey)
    |> List.map (fun (name, ls) ->
        let subPath = path </> name
        let flatedKeyValues, restKeyValues = splitKeyValues subPath ls

        let keyValues =
            let kvs = flatedKeyValues |> List.map (fun (k, v) -> k, JsonValue.Create v) |> dict
            createJsonFromFlatList spliter subPath restKeyValues
            |> Seq.fold
                (fun (state: Generic.IDictionary<_, _>) (KeyValue (k, v)) ->
                    state[k] <- v
                    state
                )
                kvs

        name, JsonValue.Create keyValues
    )
    |> dict


type State =
    {
        Spliter: string
        Keys: string list
        Jsons: Map<string, Generic.Dictionary<string, string>>
        BaseJsonName: string option
    }

    static member DefaultValue = {
        Spliter = "."
        Keys = []
        Jsons = Map.empty
        BaseJsonName = None
    }


let flatJsonList =
    html.comp (fun (hook: IComponentHook, js: IJSRuntime, snack: ISnackbar) ->
        let keysFilter = cval ""
        let state = cval State.DefaultValue

        let filteredKeys = adaptive {
            let! keys = state |> AVal.map (fun x -> x.Keys)
            let! filter = keysFilter
            if String.IsNullOrEmpty filter then
                return keys
            else
                return keys |> Seq.filter (fun x -> x.Contains(filter, StringComparison.OrdinalIgnoreCase)) |> Seq.toList

        }


        let saveAll () = task {
            try
                for KeyValue (fileName, parsedFile) in state.Value.Jsons do
                    let jsonStr =
                        parsedFile
                        |> Seq.map (fun (KeyValue (k, v)) -> k, v)
                        |> Seq.toList
                        |> createJsonFromFlatList state.Value.Spliter ""
                        |> JsonSerializer.Serialize
                    do! js.saveFile (fileName, jsonStr)
            with ex ->
                snack.Add(ex.Message, Severity.Error) |> ignore
        }


        let addFlatJson key (data: byte[]) =
            try
                state.Publish(fun state ->
                    let jsonStr = Encoding.UTF8.GetString data
                    let flattedJson = fromJsonToDict state.Spliter jsonStr
                    let isFirstJson = state.Jsons.Count = 0
                    let jsons = state.Jsons |> Map.add key flattedJson

                    { state with
                        Jsons = jsons
                        BaseJsonName = if isFirstJson then Some key else state.BaseJsonName
                        Keys = if isFirstJson then jsons[key].Keys |> Seq.toList else state.Keys
                    }
                )
            with ex ->
                snack.Add(ex.Message, Severity.Error) |> ignore


        let jsonHeader isBaseJson (jsonName: string) =
            MudTh'() {
                MudButtonGroup'() {
                    Size Size.Small
                    Color(if isBaseJson then Color.Primary else Color.Default)
                    Variant Variant.Outlined
                    MudButton'() { jsonName }
                    MudIconButton'() {
                        OnClick ignore
                        Icon Icons.Filled.Delete
                    }
                }
            }

        let jsonRowCell (keyValues: Generic.IDictionary<string, string>) jsonKey =
            MudTd'() {
                MudInput'() {
                    Value(if keyValues.ContainsKey jsonKey then keyValues[jsonKey] else "")
                    ValueChanged(fun x -> keyValues[jsonKey] <- x)
                }
            }


        section {
            MudToolBar'() {
                MudButtonGroup'() {
                    Size Size.Small
                    Variant Variant.Outlined
                    InputFile'.upload (
                        "*.json",
                        (fun (e, data) -> task { addFlatJson e.File.Name data }),
                        label' = "Upload file",
                        startIcon = Icons.Filled.Add
                    )
                    MudButton'() {
                        OnClick(ignore >> saveAll)
                        "Export all"
                    }
                }
            }
            adaptiview () {
                let! filteredKeys = filteredKeys
                let! state = state
                MudTable'() {
                    Virtualize true
                    HorizontalScrollbar true
                    Breakpoint Breakpoint.None
                    Hover true
                    Striped true
                    Dense true
                    Bordered true
                    Items filteredKeys
                    HeaderContent [
                        MudTh'() {
                            MudButtonGroup'() {
                                Size Size.Small
                                Color Color.Primary
                                Variant Variant.Filled
                                MudButton'() { "Key" }
                                MudIconButton'() {
                                    OnClick ignore
                                    Icon Icons.Filled.KeyboardArrowUp
                                }
                                MudButton'() {
                                    OnClick ignore
                                    "Add key"
                                }
                            }
                        }
                        if state.BaseJsonName.IsSome && state.Jsons.ContainsKey state.BaseJsonName.Value then
                            jsonHeader true state.BaseJsonName.Value
                        for KeyValue (jsonName, _) in state.Jsons do
                            if Some jsonName <> state.BaseJsonName then jsonHeader false jsonName
                    ]
                    RowTemplate(fun jsonKey ->
                        html.fragment [
                            MudTd'() { jsonKey }
                            if state.BaseJsonName.IsSome && state.Jsons.ContainsKey state.BaseJsonName.Value then
                                jsonRowCell state.Jsons[state.BaseJsonName.Value] jsonKey
                            for KeyValue (jsonName, keyValues) in state.Jsons do
                                if Some jsonName <> state.BaseJsonName then jsonRowCell keyValues jsonKey
                        ]
                    )
                }
            }
        }
    )
