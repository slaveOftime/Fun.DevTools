// hot-reload
namespace Fun.DevTools

open FSharp.Data.Adaptive
open Microsoft.JSInterop
open MudBlazor
open BlazorMonaco
open BlazorMonaco.Editor
open Microsoft.AspNetCore.Components.Web
open Fun.Blazor
open Fun.DevTools.HtmlConvert

type HtmlConvert' =

    static member create() =
        html.inject (fun (jsRuntime: IJSRuntime, snackbar: ISnackbar) ->
            let inputString = cval ""
            let outputString = cval ""

            let mutable inputEditorRef = Option<StandaloneCodeEditor>.None
            let mutable outputEditorRef = Option<StandaloneCodeEditor>.None

            let convert () = task {
                try
                    let result = convert inputString.Value
                    outputString.Publish result
                    match outputEditorRef with
                    | None -> ()
                    | Some e -> do! e.SetValue result
                    snackbar.Add("Converted successfully", severity = Severity.Success) |> ignore
                with ex ->
                    snackbar.Add(ex.Message, severity = Severity.Error) |> ignore
            }

            let inputEditor = MudPaper'' {
                class' "p-2"
                style {
                    overflowHidden
                    height "100%"
                }
                adapt {
                    let! f, setF = inputString.WithSetter()
                    StandaloneCodeEditor'' {
                        id "html-editor"
                        CssClass "h-full"
                        ConstructionOptions(fun _ ->
                            StandaloneEditorConstructionOptions(AutomaticLayout = true, Language = "html", Theme = "vs-dark", Value = f)
                        )
                        OnDidChangeModelContent(fun _ -> task {
                            match inputEditorRef with
                            | None -> ()
                            | Some editor ->
                                let! value = editor.GetValue()
                                setF value
                        })
                        ref (fun x -> inputEditorRef <- Some x)
                    }
                }
            }

            let outputEditor = MudPaper'' {
                class' "p-2"
                style { height "100%" }
                StandaloneCodeEditor'' {
                    id "fun-blazor-view"
                    CssClass "h-full"
                    ConstructionOptions(fun _ ->
                        StandaloneEditorConstructionOptions(
                            AutomaticLayout = true,
                            Language = "fsharp",
                            Theme = "vs-dark",
                            ReadOnly = true
                        )
                    )
                    ref (fun x -> outputEditorRef <- Some x)
                }
            }

            fragment {
                PageTitle'' { "Html Converter" }
                div {
                    style {
                        displayFlex
                        alignItemsCenter
                        justifyContentCenter
                        margin 20
                    }
                    MudText'' {
                        style {
                            textAlignCenter
                            textTransformUppercase
                            marginRight 15
                        }
                        Typo Typo.h5
                        Color Color.Primary
                        "Convert html to CE syntax"
                    }
                    MudButton'' {
                        OnClick(fun _ -> convert ())
                        Color Color.Primary
                        Variant Variant.Filled
                        "Convert"
                    }
                }
                MudGrid'' {
                    style {
                        height "100%"
                        overflowHidden
                    }
                    MudItem'' {
                        xs 6
                        style { height "100%" }
                        inputEditor
                    }
                    MudItem'' {
                        xs 6
                        style {
                            height "100%"
                            positionRelative
                        }
                        outputEditor
                        MudIconButton'' {
                            style {
                                positionAbsolute
                                right 10
                                bottom 10
                            }
                            Icon Icons.Material.Filled.ContentCopy
                            OnClick(fun _ -> task {
                                do! jsRuntime.copytoClipboard outputString.Value
                                snackbar.Add("Copied success", severity = Severity.Success) |> ignore
                            })
                        }
                    }
                }
            }
        )
