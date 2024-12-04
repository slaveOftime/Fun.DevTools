namespace Fun.DevTools

open System
open System.Text
open Fun.Blazor
open MudBlazor
open BlazorMonaco
open BlazorMonaco.Editor
open Microsoft.AspNetCore.Components.Web


type Base64Converter =

    static member Create() =
        html.inject (
            "Base64Converter",
            fun (snack: ISnackbar) ->
                let mutable inputEditorRef = ValueOption<StandaloneCodeEditor>.None
                let mutable outputEditorRef = ValueOption<StandaloneCodeEditor>.None

                let editorOptions =
                    StandaloneEditorConstructionOptions(
                        AutomaticLayout = true,
                        Language = "text",
                        Theme = "vs-dark",
                        ReadOnly = false,
                        WordWrap = "on",
                        WordBreak = "on"
                    )


                let toBase64 () = task {
                    match inputEditorRef, outputEditorRef with
                    | ValueSome inputRef, ValueSome outputRef ->
                        try
                            let! value = outputRef.GetValue()
                            do! inputRef.SetValue(Encoding.UTF8.GetBytes value |> Convert.ToBase64String)
                        with ex ->
                            snack.Add(ex.ToString(), severity = Severity.Error) |> ignore

                    | _ -> ()
                }

                let fromBase64 () = task {
                    match inputEditorRef, outputEditorRef with
                    | ValueSome inputRef, ValueSome outputRef ->
                        try
                            let sb = StringBuilder()

                            let! value = inputRef.GetValue()

                            value.Split('.', '_')
                            |> Seq.iter (fun x ->
                                try
                                    let mod4 = x.Length % 4
                                    let paddedBase64 = if mod4 > 0 then x + String('=', 4 - mod4) else x
                                    Convert.FromBase64String paddedBase64 |> Encoding.UTF8.GetString
                                with ex ->
                                    sprintf "%s\n> convert failed: %s" x ex.Message
                                |> sb.AppendLine
                                |> ignore

                                sb.AppendLine() |> ignore
                            )

                            do! outputRef.SetValue(sb.ToString())

                        with ex ->
                            snack.Add(ex.ToString(), severity = Severity.Error) |> ignore

                    | _ -> ()
                }


                fragment {
                    PageTitle'' { "Base64 Converter" }
                    div {
                        style {
                            padding 12
                            displayFlex
                            justifyContentCenter
                        }
                        MudButtonGroup'' {
                            Variant Variant.Outlined
                            MudButton'' {
                                StartIcon Icons.Material.Filled.ArrowLeft
                                OnClick(ignore >> toBase64)
                                "Encode"
                            }
                            MudButton'' {
                                EndIcon Icons.Material.Filled.ArrowRight
                                OnClick(ignore >> fromBase64)
                                "Decode"
                            }
                        }
                    }
                    MudGrid'' {
                        style {
                            height "100%"
                            overflowHidden
                        }
                        MudItem'' {
                            xs 6
                            style { overflowHidden }
                            StandaloneCodeEditor'' {
                                id "encoded"
                                CssClass "h-full"
                                ConstructionOptions(fun _ -> editorOptions)
                                ref (fun x -> inputEditorRef <- ValueSome x)
                            }
                        }
                        MudItem'' {
                            xs 6
                            style { overflowHidden }
                            StandaloneCodeEditor'' {
                                id "decoded"
                                CssClass "h-full"
                                ConstructionOptions(fun _ -> editorOptions)
                                ref (fun x -> outputEditorRef <- ValueSome x)
                            }
                        }
                    }
                }
        )
