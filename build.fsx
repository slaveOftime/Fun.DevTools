#r "nuget: BlackFox.Fake.BuildTask,0.1.3"
#r "nuget: Fake.IO.FileSystem,5.20.4"


open BlackFox.Fake
open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.IO.Globbing.Operators


fsi.CommandLineArgs |> Array.skip 1 |> BuildTask.setupContextFromArgv


let processGitHubDocs =
    BuildTask.create "ProcessGitHubDocs" [] {
        let targetDir = "Fun.DevTools.Release"
        !!(targetDir </> "**" </> "index.html")
        |> Seq.iter (File.applyReplace (fun x -> x.Replace("""<base href="/" />""", """<base href="/Fun.DevTools.Docs/" /> """)))
    }

BuildTask.runOrDefault processGitHubDocs
