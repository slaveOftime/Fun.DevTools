[<AutoOpen>]
module Fun.DevTools.Hooks

open Microsoft.Extensions.DependencyInjection
open Fun.Blazor


type IComponentHook with

    member hook.ShareStore = hook.ServiceProvider.GetService<IShareStore>()
