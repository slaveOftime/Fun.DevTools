open System
open System.Text.Json.Serialization
open Microsoft.Extensions.DependencyInjection
open Microsoft.AspNetCore.Components.WebAssembly.Hosting
open MudBlazor.Services
open Blazored.LocalStorage
open Fun.Blazor
open Fun.DevTools


let builder = WebAssemblyHostBuilder.CreateDefault(Environment.GetCommandLineArgs())

#if DEBUG
builder.AddFunBlazor("#app", html.hotReloadComp (app, "Fun.DevTools.App.app")) |> ignore
#else
builder.AddFunBlazor("#app", app) |> ignore
#endif

builder
    .Services
    .AddFunBlazorWasm()
    .AddMudServices()
    .AddBlazoredLocalStorage(fun options -> options.JsonSerializerOptions.Converters.Add(JsonFSharpConverter()))
|> ignore


builder.Build().RunAsync() |> ignore
