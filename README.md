Tips:

1. Check the line in file Fun.Json.Tools.fsproj: 

    It required MudBlazor. Also has attribute FunBlazorStyle.

    ```
    <PackageReference Include="MudBlazor" FunBlazorStyle="CE" Version="6.0.10" />
    ```

2. dotnet tool install --global Fun.Blazor.Cli --version 2.0.*

    If you change the version of MudBlazor you should run "fun-blazor generate" under this folder to regenerate Fun.Blazor.Bindings

3. According to MudBlazor, you should also add related css/js resources in wwwroot/index.html

4. The reason we split MudBlazor bindings into a separate project is for trimming to get smaller bundle size.  

## Dev with hot-reload

    Open terminal and run
    dotnet run --project .\Fun.Json.Tools\Fun.Json.Tools.fsproj
    
    Open terminal and run
    fun-blazor watch .\Fun.Json.Tools\Fun.Json.Tools.fsproj

    > require: dotnet tool install --global Fun.Blazor.Cli --version 2.0.*
