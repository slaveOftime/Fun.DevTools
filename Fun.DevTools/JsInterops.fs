[<AutoOpen>]
module Fun.DevTools.JsInterops

open Microsoft.JSInterop


type IJSRuntime with

    member this.saveFile(filename: string, data: string) = this.InvokeVoidAsync("utils.saveFile", filename, data)

    member this.copytoClipboard(value: string) = this.InvokeVoidAsync("utils.copytoClipboard", value)
