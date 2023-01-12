// hot-reload
namespace Fun.DevTools

open FSharp.Data.Adaptive
open MudBlazor
open Fun.Blazor
open Fun.DevTools.HtmlToFunBlazor


type HtmlToFunBlazor' =

    static member create () =
        let inputString = cval ""
        let outputString = cval ""

        fragment {
            MudText'() {
                style {
                    marginTop 20
                    textAlignCenter
                    textTransformUppercase
                }
                Typo Typo.h5
                Color Color.Primary
                "Convert html to Fun.Blazor sytax"
            }
            div {
                style {
                    displayFlex
                    alignItemsCenter
                    justifyContentCenter
                    margin 20
                }
                MudButton'() {
                    OnClick(fun _ ->
                        try convert inputString.Value |> outputString.Publish
                        with ex -> outputString.Publish ex.Message
                    )
                    Color Color.Primary
                    Variant Variant.Filled
                    "Convert"
                }
            }
            MudGrid'() {
                childContent [
                    MudItem'() {
                        xs 6
                        MudPaper'() {
                            class' "p-2"
                            style {
                                overflowHidden
                            }
                            adaptiview() {
                                let! f, setF = inputString.WithSetter()
                                textarea {
                                    style {
                                        minHeight 500
                                        width "100%"
                                        padding 10
                                        resizeNone
                                    }
                                    value f
                                    onchange (fun e -> e.Value |> string |> setF)
                                }
                            }
                        }
                    }
                    MudItem'() {
                        xs 6
                        MudPaper'() {
                            class' "p-2"
                            style {
                                height 500
                                overflowAuto
                            }
                            adaptiview() {
                                let! f = outputString
                                pre {
                                    style {
                                        height "100%"
                                        padding 10
                                    }
                                    f
                                }
                            }
                        }
                    }
                ]
            }
        }
