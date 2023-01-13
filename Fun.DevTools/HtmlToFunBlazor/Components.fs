// hot-reload
namespace Fun.DevTools

open FSharp.Data.Adaptive
open Microsoft.JSInterop
open MudBlazor
open BlazorMonaco
open Fun.Blazor
open Fun.DevTools.HtmlToFunBlazor

type HtmlToFunBlazor' =

    static member create () = html.inject (fun (jsRuntime: IJSRuntime, snackbar: ISnackbar) ->
        let inputString = cval ""
        let outputString = cval ""

        let mutable inputEditorRef = Option<MonacoEditor>.None
        let mutable outputEditorRef = Option<MonacoEditor>.None

        let inputEditor =
            MudPaper'() {
                class' "p-2"
                style {
                    overflowHidden
                    height "100%"
                }
                adaptiview() {
                    let! f, setF = inputString.WithSetter()
                    MonacoEditor'() {
                        id "html-editor"
                        CssClass "h-full"
                        ConstructionOptions(fun _ ->
                            StandaloneEditorConstructionOptions(
                                AutomaticLayout = true,
                                Language = "html",
                                Theme = "vs-dark",
                                Value = f
                            )
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

        let outputEditor =
            MudPaper'() {
                class' "p-2"
                style {
                    height "100%"
                }
                MonacoEditor'() {
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

        html.fragment [
            MudText'() {
                style {
                    marginTop 20
                    textAlignCenter
                    textTransformUppercase
                }
                Typo Typo.h5
                Color Color.Primary
                "Convert html to Fun.Blazor syntax"
            }
            div {
                style {
                    displayFlex
                    alignItemsCenter
                    justifyContentCenter
                    margin 20
                }
                MudButton'() {
                    OnClick(fun _ -> task {
                        try 
                            let result = convert inputString.Value 
                            outputString.Publish result
                            match outputEditorRef with
                            | None -> ()
                            | Some e -> do! e.SetValue result
                        with ex ->
                            outputString.Publish ex.Message
                    })
                    Color Color.Primary
                    Variant Variant.Filled
                    "Convert"
                }
            }
            MudGrid'() {
                style {
                    height "100%"
                    overflowHidden
                }
                childContent [
                    MudItem'() {
                        xs 6
                        style {
                            height "100%"
                        }
                        inputEditor
                    }
                    MudItem'() {
                        xs 6
                        style {
                            height "100%"
                            positionRelative
                        }
                        outputEditor
                        MudIconButton'() {
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
                ]
            }
        ]
    )
