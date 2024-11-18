open System
open System.Text.Json.Serialization
open Microsoft.Extensions.DependencyInjection
open Microsoft.AspNetCore.Components.WebAssembly.Hosting
open Microsoft.AspNetCore.Components.Web
open MudBlazor.Services
open Blazored.LocalStorage
open Fun.DevTools


let builder = WebAssemblyHostBuilder.CreateDefault(Environment.GetCommandLineArgs())

builder.AddFunBlazor("#app", app) |> ignore
builder.RootComponents.Add<HeadOutlet>("head::after")

builder.Services
    .AddFunBlazorWasm()
    .AddMudServices()
    .AddBlazoredLocalStorage(fun options -> options.JsonSerializerOptions.Converters.Add(JsonFSharpConverter()))
|> ignore


builder.Build().RunAsync() |> ignore
