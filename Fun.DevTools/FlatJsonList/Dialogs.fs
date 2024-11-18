// hot-reload
[<AutoOpen>]
module Fun.DevTools.FlatJsonList.Dialogs

open System
open FSharp.Data.Adaptive
open MudBlazor
open Fun.Blazor
open Fun.DevTools.Controls
open Fun.DevTools.FlatJsonList


let openAddNewKeyDialog (hook: IComponentHook) =
    let dialog = hook.ServiceProvider.GetMultipleServices<IDialogService>()
    dialog.Show(fun ctx ->
        let keyValue = cval ("")
        MudDialog'' {
            Options(DialogOptions(MaxWidth = MaxWidth.Small))
            DialogContent(
                adapt {
                    let! binding = keyValue.WithSetter()
                    MudTextField'' {
                        style { width "300px" }
                        Label "Input and press Enter to add new key"
                        Value' binding
                        AutoFocus true
                        Immediate true
                    }
                }
            )
            DialogActions [
                MudButton'' {
                    OnClick(ignore >> ctx.Close)
                    "Close"
                }
                adapt {
                    let! v = keyValue
                    MudButton'' {
                        Disabled(String.IsNullOrEmpty v)
                        Variant Variant.Filled
                        Color Color.Primary
                        OnClick(fun _ ->
                            ctx.Close()
                            hook.State.Publish(fun state -> { state with Keys = v :: state.Keys |> List.distinct })
                        )
                        "Confirm"
                    }
                }
            ]
        }
    )


let openDetailEditorDialog (hook: IComponentHook) (jsonKey: string) =
    let dialog = hook.ServiceProvider.GetMultipleServices<IDialogService>()
    dialog.Show(
        DialogOptions(MaxWidth = MaxWidth.Medium, FullWidth = true),
        fun ctx -> MudDialog'' {
            TitleContent(
                MudText'' {
                    Color Color.Warning
                    jsonKey
                }
            )
            DialogContent(
                div {
                    style {
                        maxHeight 500
                        overflowYAuto
                    }
                    adapt {
                        let! jsons = hook.State |> AVal.map (fun x -> Map.toList x.Jsons |> List.sortBy fst)
                        for k, keyValues in jsons do
                            MudTextField'' {
                                style {
                                    width "100%"
                                    marginBottom 15
                                }
                                Label k
                                Value(if keyValues.ContainsKey jsonKey then keyValues[jsonKey] else "")
                                ValueChanged(fun x ->
                                    hook.JsonKeyRefresher.Publish(fun _ -> jsonKey, Random.Shared.Next())
                                    keyValues[jsonKey] <- x
                                )
                                Lines 5
                                AutoFocus true
                                FullWidth true
                            }
                    }
                }
            )
            DialogActions [|
                MudButton'' {
                    OnClick(ignore >> ctx.Close)
                    "Close"
                }
            |]
        }
    )


let openConfirmDeleteDialog (hook: IComponentHook) (jsonKey: string) =
    let dialog = hook.ServiceProvider.GetMultipleServices<IDialogService>()
    dialog.Show(fun ctx -> MudDialog'' {
        DialogContent(
            MudText'' {
                Color Color.Warning
                $"Do you confirm to delete key: {jsonKey}"
            }
        )
        DialogActions [|
            MudButton'' {
                OnClick(ignore >> ctx.Close)
                "Close"
            }
            MudButton'' {
                Variant Variant.Filled
                Color Color.Primary
                OnClick(fun _ ->
                    ctx.Close()
                    hook.State.Publish(fun state -> {
                        state with
                            Keys = state.Keys |> List.filter ((<>) jsonKey)
                            Jsons =
                                state.Jsons
                                |> Map.map (fun _ kv ->
                                    kv.Remove jsonKey |> ignore
                                    kv
                                )
                    })
                )
                "Confirm"
            }
        |]
    })
