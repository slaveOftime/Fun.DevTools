namespace Fun.Json.Tools.Controls

open System.IO
open Microsoft.AspNetCore.Components.Forms
open Fun.Blazor
open Fun.Blazor.Operators
open MudBlazor


type InputFile' =

    static member upload(accept': string, upload, ?label': string, ?startIcon, ?size', ?fullwidth, ?variant, ?onError, ?isDisabled) =
        let inputId = "upload-file-" + (defaultArg label' "").GetHashCode().ToString()
        let isDisabled = defaultArg isDisabled false
        
        html.fragment [
            InputFile'() {
                "hidden" => "true"
                id inputId
                accept accept'
                OnChange(fun e -> task {
                    try
                        let stream = e.File.OpenReadStream(maxAllowedSize = 1024L * 1024L * 15L)
                        use ms = new MemoryStream(int stream.Length)
                        do! stream.CopyToAsync(ms)
                        let data = ms.ToArray()
                        do! upload (e, data)
                    with ex ->
                        (defaultArg onError ignore) (ex)
                }
                )
            }
            match label' with
            | Some label' ->
                MudButton'() {
                    ``for`` inputId
                    HtmlTag "label"
                    Size(defaultArg size' Size.Small)
                    StartIcon(defaultArg startIcon Icons.Filled.Upload)
                    FullWidth(defaultArg fullwidth false)
                    Variant(defaultArg variant Variant.Text)
                    Disabled isDisabled
                    label'
                }
            | None ->
                MudIconButton'() {
                    ``for`` inputId
                    HtmlTag "label"
                    Size(defaultArg size' Size.Small)
                    Icon(defaultArg startIcon Icons.Filled.Upload)
                    Variant(defaultArg variant Variant.Text)
                    Disabled isDisabled
                }
        ]
