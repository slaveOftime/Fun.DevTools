[<AutoOpen>]
module Fun.DevTools.FlatJsonList.Utils

open System
open System.Text.Json
open System.Text.Json.Nodes
open System.Text.Unicode
open System.Text.Encodings.Web
open System.Collections


let jsonSerializeOptions = JsonSerializerOptions(WriteIndented = true, Encoder = JavaScriptEncoder.Create UnicodeRanges.All)


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


let fromJsonToDict spliter (json: string) =
    let doc = JsonDocument.Parse(json)
    let dict = Generic.Dictionary()
    flatJson spliter dict doc.RootElement ""
    dict


let rec createJsonFromFlatList spliter (path: string) (parsedFile: List<string * string>) =
    let inline (</>) str1 str2 =
        if String.IsNullOrEmpty str1 then str2
        elif String.IsNullOrEmpty str2 then str1
        else str1 + spliter + str2

    let inline getKey path (key: string) =
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
    |> List.groupBy (fst >> getKey path)
    |> List.map (fun (name, ls) ->
        match ls with
        | [ k, v ] when name = k -> Generic.KeyValuePair(name, JsonValue.Create(v))
        | _ ->
            let subPath = path </> name
            let flatedKeyValues, restKeyValues = splitKeyValues subPath ls

            let keyValues =
                let kvs =
                    flatedKeyValues
                    |> List.map (fun (k, v) -> Generic.KeyValuePair(getKey subPath k, JsonValue.Create v))
                    |> Generic.Dictionary
                createJsonFromFlatList spliter subPath restKeyValues
                |> Seq.fold
                    (fun (state: Generic.Dictionary<_, _>) (KeyValue(k, v)) ->
                        state.Add(k, v)
                        state
                    )
                    kvs

            Generic.KeyValuePair(name, JsonValue.Create keyValues)
    )
    |> Generic.Dictionary
