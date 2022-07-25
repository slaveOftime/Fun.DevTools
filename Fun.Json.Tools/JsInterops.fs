[<AutoOpen>]
module Fun.Json.Tools.JsInterops

open Microsoft.JSInterop


type IJSRuntime with

    member this.saveFile(filename: string, data: string) = this.InvokeVoidAsync("utils.saveFile", filename, data)
