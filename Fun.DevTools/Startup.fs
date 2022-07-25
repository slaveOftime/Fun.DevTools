open System
open Microsoft.Extensions.DependencyInjection
open Microsoft.AspNetCore.Components.WebAssembly.Hosting
open MudBlazor.Services
open Fun.Blazor
open Fun.DevTools

let builder = WebAssemblyHostBuilder.CreateDefault(Environment.GetCommandLineArgs())

#if DEBUG
builder.AddFunBlazor("#app", html.hotReloadComp(app, "Fun.DevTools.App.app")) |> ignore
#else
builder.AddFunBlazor("#app", app) |> ignore
#endif

builder.Services.AddFunBlazorWasm().AddMudServices() |> ignore


builder.Build().RunAsync() |> ignore
