// hot-reload
[<AutoOpen>]
module Fun.Json.Tools.App

open Fun.Blazor
open MudBlazor


let app = div {
    MudThemeProvider'()
    MudDialogProvider'()
    MudSnackbarProvider'()
    MudContainer'() {
        MudText'() {
            style { textAlignCenter }
            Typo Typo.h4
            Color Color.Primary
            "Tools for JSON"
        }
        MudText'() {
            style { textAlignCenter }
            Typo Typo.subtitle1
            "Flat and list json key values which can be used for translation"
        }
        flatJsonList
    }
}
