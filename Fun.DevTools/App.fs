// hot-reload
[<AutoOpen>]
module Fun.DevTools.App

open Fun.Blazor
open MudBlazor


let app = div {
    style { height "100%" }
    MudThemeProvider'()
    MudDialogProvider'()
    MudSnackbarProvider'()
    MudContainer'() {
        style {
            height "100%"
            overflowHidden
            displayFlex
            flexDirectionColumn
        }
        FlatJsonList'.create ()
    }
}
