// hot-reload
namespace Fun.DevTools

open System.Collections
open FSharp.Data.Adaptive
open MudBlazor
open Fun.Result
open Fun.Blazor
open Fun.DevTools.Controls
open Fun.DevTools.FlatJsonList


type FlatJsonList' =
    static member create() =
        html.comp (fun (hook: IComponentHook, dialog: IDialogService) ->
            hook.InitFlatJsonList()
            
            let addNewKey () =
                dialog.Show(fun ctx ->
                    MudDialog'() {
                        Options(DialogOptions(MaxWidth = MaxWidth.Small))
                        DialogContent(
                            MudTextField'() {
                                style { width "300px" }
                                Label "Input and press Enter to add new key"
                                ValueChanged(
                                    function
                                    | SafeString x ->
                                        ctx.Close()
                                        hook.State.Publish(fun state -> { state with Keys = x :: state.Keys |> List.distinct })
                                    | _ -> ()
                                )
                            }
                        )
                        DialogActions [
                            MudButton'() {
                                OnClick(ignore >> ctx.Close)
                                "Close"
                            }
                        ]
                    }
                )


            let keyHeader =
                MudTh'() {
                    MudButtonGroup'() {
                        Size Size.Small
                        Color Color.Primary
                        Variant Variant.Text
                        adaptiview (key = nameof hook.KeysFilter) {
                            let! binding = hook.KeysFilter.WithSetter()
                            MudInput'() {
                                Value' binding
                                Placeholder "Key"
                                Adornment Adornment.Start
                                AdornmentIcon Icons.Filled.FilterAlt
                                IconSize Size.Small
                            }
                        }
                        adaptiview (key = nameof hook.KeysSortIsASC) {
                            let! isAsc, setAsc = hook.KeysSortIsASC.WithSetter()
                            MudIconButton'() {
                                OnClick(fun _ -> setAsc (not isAsc))
                                Icon(if isAsc then Icons.Filled.KeyboardArrowDown else Icons.Filled.KeyboardArrowUp)
                            }
                        }
                        MudIconButton'() {
                            OnClick(ignore >> addNewKey)
                            Icon Icons.Filled.Add
                        }
                    }
                }


            let jsonHeader isBaseJson (jsonName: string) =
                MudTh'() {
                    style { minWidth "200px" }
                    MudText'() {
                        Color(if isBaseJson then Color.Primary else Color.Default)
                        jsonName
                    }
                }


            let jsonRowCell (keyValues: Generic.IDictionary<string, string>) jsonKey =
                MudTd'() {
                    MudInput'() {
                        style { width "100%" }
                        Value(if keyValues.ContainsKey jsonKey then keyValues[jsonKey] else "")
                        ValueChanged(fun x -> keyValues[jsonKey] <- x)
                        FullWidth true
                    }
                }


            fragment {
                MudText'() {
                    style {
                        marginTop 20
                        textAlignCenter
                    }
                    Typo Typo.h5
                    Color Color.Primary
                    "Flat multiple JSON files to compare"
                }
                div {
                    style {
                        flexShrink 0
                        displayFlex
                        alignItemsCenter
                        justifyContentCenter
                        marginTop 10
                        marginBottom 15
                    }
                    MudButtonGroup'() {
                        Size Size.Small
                        Variant Variant.Outlined
                        Color Color.Primary
                        InputFile'.upload (
                            "*.json",
                            (fun (e, data) -> task { hook.AddFlatJson(e.File.Name, data) }),
                            label' = "Add file",
                            startIcon = Icons.Filled.Add
                        )
                        MudButton'() {
                            OnClick(ignore >> hook.ExportAll)
                            StartIcon Icons.Filled.SaveAlt
                            "Export all"
                        }
                        MudButton'() {
                            OnClick(ignore >> hook.ClearAll)
                            StartIcon Icons.Filled.ClearAll
                            "Clear"
                        }
                    }
                }
                adaptiview () {
                    let! jsons = hook.State |> AVal.map (fun x -> x.Jsons)
                    let! baseJsonName = hook.State |> AVal.map (fun x -> x.BaseJsonName)
                    let! filteredKeys = hook.FilteredKeys
                    MudTable'() {
                        style {
                            height "100%"
                            marginBottom 10
                            overflowHidden
                        }
                        Height "100%"
                        Elevation 10
                        Virtualize true
                        FixedHeader true
                        HorizontalScrollbar true
                        Breakpoint Breakpoint.None
                        Hover true
                        Striped true
                        Dense true
                        Bordered true
                        Items filteredKeys
                        HeaderContent [
                            keyHeader
                            if baseJsonName.IsSome && jsons.ContainsKey baseJsonName.Value then
                                jsonHeader true baseJsonName.Value
                            for KeyValue (jsonName, _) in jsons do
                                if Some jsonName <> baseJsonName then jsonHeader false jsonName
                        ]
                        RowTemplate(fun jsonKey ->
                            html.fragment [
                                MudTd'() { jsonKey }
                                if baseJsonName.IsSome && jsons.ContainsKey baseJsonName.Value then
                                    jsonRowCell jsons[baseJsonName.Value] jsonKey
                                for KeyValue (jsonName, keyValues) in jsons do
                                    if Some jsonName <> baseJsonName then jsonRowCell keyValues jsonKey
                            ]
                        )
                    }
                }
                div {
                    style {
                        maxWidth 720
                        margin "auto"
                        marginBottom 10
                    }
                    ol {
                        li {
                            MudText'() {
                                Typo Typo.subtitle2
                                Color Color.Warning
                                "The first uploaded file will be used as the base file which will provide the key column."
                            }
                        }
                        li {
                            adaptiview () {
                                let! spliter = hook.State |> AVal.map (fun x -> x.Spliter)
                                MudText'() {
                                    Typo Typo.subtitle2
                                    Color Color.Warning
                                    $"{spliter} is the splitter, so you should not use it in your key."
                                }
                            }
                        }
                        li {
                            MudText'() {
                                Typo Typo.subtitle2
                                Color Color.Info
                                "Flat and list json key values which can be used for translation files. Only UTF-8 file is supportted!"
                            }
                        }
                        li {
                            MudText'() {
                                Typo Typo.subtitle2
                                Color Color.Info
                                "All the json values will be treated as string."
                            }
                        }
                    }
                }
            }
        )
