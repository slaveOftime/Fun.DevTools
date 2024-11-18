// hot-reload
namespace Fun.DevTools

open System.Collections
open FSharp.Data.Adaptive
open MudBlazor
open Fun.Blazor
open Microsoft.AspNetCore.Components.Web
open Fun.DevTools.Controls
open Fun.DevTools.FlatJsonList


type FlatJsonList' =

    static member create() =
        html.inject (fun (hook: IComponentHook) ->
            hook.InitFlatJsonList()


            let mainActions = div {
                style {
                    flexShrink 0
                    displayFlex
                    alignItemsCenter
                    justifyContentCenter
                    marginTop 10
                    marginBottom 15
                }
                MudButtonGroup'' {
                    Size Size.Small
                    Variant Variant.Filled
                    Color Color.Primary
                    InputFile'.upload (
                        "*.json",
                        (fun (e, data) -> task { hook.AddFlatJson(e.File.Name, data) }),
                        label' = "Add file",
                        startIcon = Icons.Material.Filled.Add
                    )
                    MudButton'' {
                        OnClick(ignore >> hook.ExportAll)
                        StartIcon Icons.Material.Filled.SaveAlt
                        "Export all"
                    }
                    MudButton'' {
                        OnClick(ignore >> hook.ClearAll)
                        StartIcon Icons.Material.Filled.ClearAll
                        "Clear"
                    }
                }
            }


            let keyHeader = MudTh'' {
                MudButtonGroup'' {
                    Size Size.Small
                    Color Color.Primary
                    Variant Variant.Text
                    adaptiview (key = nameof hook.KeysFilter) {
                        let! binding = hook.KeysFilter.WithSetter()
                        MudInput'' {
                            Value' binding
                            Placeholder "Key"
                            Adornment Adornment.Start
                            AdornmentIcon Icons.Material.Filled.FilterAlt
                            IconSize Size.Small
                        }
                    }
                    adaptiview (key = nameof hook.KeysSortIsASC) {
                        let! isAsc, setAsc = hook.KeysSortIsASC.WithSetter()
                        MudIconButton'' {
                            OnClick(fun _ -> setAsc (not isAsc))
                            Icon(
                                if isAsc then
                                    Icons.Material.Filled.KeyboardArrowDown
                                else
                                    Icons.Material.Filled.KeyboardArrowUp
                            )
                        }
                    }
                    MudIconButton'' {
                        OnClick(fun _ -> openAddNewKeyDialog hook)
                        Icon Icons.Material.Filled.Add
                    }
                }
            }

            let jsonHeader isBaseJson (jsonName: string) = MudTh'' {
                style { minWidth "200px" }
                MudText'' {
                    Color(if isBaseJson then Color.Primary else Color.Default)
                    jsonName
                }
            }

            let jsonKeyRowCell (jsonKey: string) = MudTd'' {
                style {
                    displayFlex
                    alignItemsCenter
                    justifyContentSpaceBetween
                }
                span {
                    style { flexGrow 1 }
                    jsonKey
                }
                MudButtonGroup'' {
                    style { marginLeft 10 }
                    Size Size.Small
                    Variant Variant.Text
                    MudIconButton'' {
                        OnClick(fun _ -> openConfirmDeleteDialog hook jsonKey)
                        Icon Icons.Material.Filled.Clear
                    }
                    MudIconButton'' {
                        OnClick(fun _ -> openDetailEditorDialog hook jsonKey)
                        Icon Icons.Material.Filled.EditNote
                    }
                }
            }

            let jsonRowCell (keyValues: Generic.IDictionary<string, string>) jsonKey = MudTd'' {
                adapt {
                    let! _ = hook.JsonKeyRefresher |> AVal.map (fun (k, r) -> if k = jsonKey then r else 0)
                    MudInput'' {
                        style { width "100%" }
                        Value(if keyValues.ContainsKey jsonKey then keyValues[jsonKey] else "")
                        ValueChanged(fun x -> keyValues[jsonKey] <- x)
                        FullWidth true
                    }
                }
            }


            let table = adapt {
                let! jsons = hook.State |> AVal.map (fun x -> x.Jsons)
                let! baseJsonName = hook.State |> AVal.map (fun x -> x.BaseJsonName)
                let! filteredKeys = hook.FilteredKeys
                MudTable'' {
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
                    HeaderContent [|
                        keyHeader
                        if baseJsonName.IsSome && jsons.ContainsKey baseJsonName.Value then
                            jsonHeader true baseJsonName.Value
                        for KeyValue(jsonName, _) in jsons do
                            if Some jsonName <> baseJsonName then jsonHeader false jsonName
                    |]
                    RowTemplate(fun jsonKey ->
                        html.fragment [
                            jsonKeyRowCell jsonKey
                            if baseJsonName.IsSome && jsons.ContainsKey baseJsonName.Value then
                                jsonRowCell jsons[baseJsonName.Value] jsonKey
                            for KeyValue(jsonName, keyValues) in jsons do
                                if Some jsonName <> baseJsonName then jsonRowCell keyValues jsonKey
                        ]
                    )
                }
            }


            let tips = div {
                style {
                    maxWidth 720
                    margin "auto"
                    marginBottom 10
                }
                ol {
                    li {
                        MudText'' {
                            Typo Typo.subtitle2
                            Color Color.Warning
                            "The first uploaded file will be used as the base file which will provide the key column."
                        }
                    }
                    li {
                        adapt {
                            let! spliter = hook.State |> AVal.map (fun x -> x.Spliter)
                            MudText'' {
                                Typo Typo.subtitle2
                                Color Color.Warning
                                $"{spliter} is the splitter, so you should not use it in your key."
                            }
                        }
                    }
                    li {
                        MudText'' {
                            Typo Typo.subtitle2
                            Color Color.Info
                            "Flat and list json key values which can be used for translation files. Only UTF-8 file is supportted!"
                        }
                    }
                    li {
                        MudText'' {
                            Typo Typo.subtitle2
                            Color Color.Info
                            "All the json values will be treated as string."
                        }
                    }
                }
            }


            fragment {
                PageTitle'' { "Flat Json" }
                MudText'' {
                    style {
                        marginTop 20
                        textAlignCenter
                        textTransformUppercase
                    }
                    Typo Typo.h5
                    Color Color.Primary
                    "Flat multiple JSON files to compare"
                }
                mainActions
                table
                tips
            }
        )
