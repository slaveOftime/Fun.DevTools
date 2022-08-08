module Fun.DevTools.Tests.FlatJsonListTests

open System.Text.Json
open Xunit
open Fun.DevTools.FlatJsonList


let expectedJson =
    """{
  "app": {
    "title": "\u003Ch1\u003Ewintool\u003C/h1\u003E",
    "version": "0.0.1",
    "msg": {
      "description": "de1",
      "description2": "de2"
    }
  },
  "msg": {
    "error": "error"
  }
}"""


let expectedFlattedKeyValues = [
    "app.title", "<h1>wintool</h1>"
    "app.version", "0.0.1"
    "app.msg.description", "de1"
    "app.msg.description2", "de2"
    "msg.error", "error"
]


[<Fact>]
let ``createJsonFromFlatList should work`` () =
    let actual =
        createJsonFromFlatList "." "" expectedFlattedKeyValues |> fun x -> JsonSerializer.Serialize(x, jsonSerializeOptions)

    Assert.Equal(expectedJson, actual)


[<Fact>]
let ``fromJsonToDict should work`` () =
    let actual = fromJsonToDict "." expectedJson |> Seq.map (fun (KeyValue (k, v)) -> k, v) |> Seq.toList
    Assert.Equal<string * string>(expectedFlattedKeyValues, actual)
