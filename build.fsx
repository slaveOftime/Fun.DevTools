#r "nuget: Fun.Build"
#r "nuget: Fake.IO.FileSystem,5.20.4"


open Fun.Build
open Fun.Build.Github
open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.IO.Globbing.Operators


pipeline "deploy" {
    stage "test" { run "dotnet test" }
    stage "bundle" { run "dotnet publish Fun.DevTools/Fun.DevTools.fsproj -c Release -o Fun.DevTools.Release --nologo" }
    stage "prepare assets for github" {
        whenGithubAction
        run (fun _ ->
            printfn "Change base-tag in index.html"
            let targetDir = "Fun.DevTools.Release"
            !!(targetDir </> "**" </> "index.html")
            |> Seq.iter (File.applyReplace (fun x -> x.Replace("""<base href="/" />""", """<base href="/Fun.DevTools.Docs/" /> """)))

            printfn "copy index.html to 404.html to serve the same file when a file is not found"
            File.read "Fun.DevTools.Release/wwwroot/index.html" |> File.write false "Fun.DevTools.Release/wwwroot/404.html"

            printfn
                "add .nojekyll file to tell GitHub pages to not treat this as a Jekyll project. (Allow files and folders starting with an underscore)"
            File.create "Fun.DevTools.Release/wwwroot/.nojekyll"
        )
    }
    runIfOnlySpecified
}


tryPrintPipelineCommandHelp ()
