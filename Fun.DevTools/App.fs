[<AutoOpen>]
module Fun.DevTools.App

open Microsoft.AspNetCore.Components.Web
open Microsoft.AspNetCore.Components.Routing
open Fun.Blazor
open Fun.Blazor.Router
open MudBlazor

let app = div {
    style { height "100%" }

    MudThemeProvider'' {
        Theme(
            MudTheme(
                PaletteDark =
                    PaletteDark(Primary = "#596b94", Secondary = "#2e6f91", Black = "#0d1121", Surface = "#18202c", Background = "#161926")
            )
        )
        IsDarkMode
    }

    MudDialogProvider''
    MudSnackbarProvider''

    ErrorBoundary'' {
        ErrorContent(fun ex -> MudPaper'' {
            style {
                padding 10
                margin 20
            }
            Elevation 10
            MudAlert'' {
                Severity Severity.Error
                ex.Message
            }
        })
        MudLayout'' {
            MudDrawer'' {
                Open true
                Fixed true
                Elevation 5
                Variant DrawerVariant.Persistent
                MudDrawerHeader'' {
                    MudText'' {
                        Typo Typo.h5
                        Color Color.Primary
                        "Fun Dev Tool"
                    }
                }
                MudNavLink'' {
                    Match NavLinkMatch.All
                    Href ""
                    "Html convert"
                }
                MudNavLink'' {
                    Match NavLinkMatch.All
                    Href "flat-json"
                    "Flat Json List"
                }
                MudNavLink'' {
                    Match NavLinkMatch.All
                    Href "base64"
                    "Base64 convert"
                }
            }
            MudMainContent'' {
                style {
                    height "100%"
                    overflowHidden
                    displayFlex
                    flexDirectionColumn
                    padding 10
                }
                html.route [|
                    let routes = [ routeCi "/flat-json" (FlatJsonList'.create ()); routeCi "/base64" (Base64Converter.Create()) ]
                    yield! routes
                    subRouteCi "/Fun.DevTools.Docs" routes
                    routeAny (HtmlConvert'.create ())
                |]
            }
        }
    }
}
