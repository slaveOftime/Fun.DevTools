// hot-reload
[<AutoOpen>]
module Fun.DevTools.App

open Microsoft.AspNetCore.Components.Web
open Microsoft.AspNetCore.Components.Routing
open Fun.Blazor
open Fun.Blazor.Router
open MudBlazor


let primaryColor = "#495ca5"
let secondaryColor = "#6677BB"


let defaultTheme = MudTheme(Palette = Palette(Primary = primaryColor, Secondary = secondaryColor, Black = "#202120"))


let darkTheme =
    MudTheme(
        Palette =
            Palette(
                Primary = primaryColor,
                Secondary = secondaryColor,
                Surface = "#1e1e2d",
                Background = "#1a1a27",
                BackgroundGrey = "#151521",
                AppbarText = "#92929f",
                AppbarBackground = "rgba(26,26,39,0.8)",
                DrawerBackground = "#1a1a27",
                ActionDefault = "#74718e",
                ActionDisabled = "#9999994d",
                ActionDisabledBackground = "#605f6d4d",
                TextPrimary = "#b2b0bf",
                TextSecondary = "#92929f",
                TextDisabled = "#ffffff33",
                DrawerIcon = "#92929f",
                DrawerText = "#92929f",
                GrayLight = "#2a2833",
                GrayLighter = "#1e1e2d",
                Info = "#4a86ff",
                Success = "#3dcb6c",
                Warning = "#ffb545",
                Error = "#ff3f5f",
                LinesDefault = "#33323e",
                TableLines = "#33323e",
                Divider = "#292838",
                OverlayLight = "#1e1e2d80"
            )
    )


let app = div {
    style { height "100%" }
    MudThemeProvider'() { Theme darkTheme }
    MudDialogProvider'()
    MudSnackbarProvider'()
    ErrorBoundary'() {
        ErrorContent(fun ex ->
            MudPaper'() {
                style {
                    padding 10
                    margin 20
                }
                Elevation 10
                MudAlert'() {
                    Severity Severity.Error
                    ex.Message
                }
            }
        )
        childContent [
            MudLayout'() {
                childContent [
                    MudDrawer'() {
                        Open true
                        Fixed true
                        Elevation 5
                        Variant DrawerVariant.Persistent
                        childContent [
                            MudDrawerHeader'() {
                                MudText'() {
                                    Typo Typo.h5
                                    Color Color.Primary
                                    "Fun Dev Tool"
                                }
                            }
                            MudNavLink'() {
                                Match NavLinkMatch.All
                                Href ""
                                "Flat Json List"
                            }
                            MudNavLink'() {
                                Match NavLinkMatch.All
                                Href "html-to-fun-blazor"
                                "Html to Fun.Blazor"
                            }
                        ]
                    }
                    MudMainContent'() {
                        style {
                            height "100%"
                            overflowHidden
                            displayFlex
                            flexDirectionColumn
                            padding 10
                        }
                        html.route [
                            routeCi "/html-to-fun-blazor" (HtmlToFunBlazor'.create())
                            routeAny (FlatJsonList'.create ())
                        ]
                    }
                ]
            }
        ]
    }
}
