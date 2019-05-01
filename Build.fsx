#r "paket: groupref FakeBuild //"
#load "./.fake/build.fsx/intellisense.fsx"

open Fake.IO
open Fake.BuildServer
open Fake.IO.Globbing.Operators
open Fake.Core
open Fake.DotNet
open Fake.Core
open Fake.Core
open Fake.Core

BuildServer.install [
    AppVeyor.Installer
]

let isAppveyor = AppVeyor.detect()
let forceBumpVersion = Environment.hasEnvironVar "BumpVersion"
let forceBuildNumber = Environment.environVarOrDefault "BuildNumber"

let runBumpVersion = forceBumpVersion

Target.create "Clean" (fun _ ->
  ["build"]
  |> Seq.iter Directory.delete
)

Target.create "BumpVersion" (fun _ ->
    Trace.logfn "Bumping Version"
    Shell.Exec("powershell.exe", "-NoProfile -ExecutionPolicy Bypass -File scripts\\Bump-Version.ps1 -BumpBuild -BuildNumber:" + "asdf") |> ignore
    ()
)

Target.create "Build" (fun _ ->
    (*
    let setParams (defaults:MSBuildParams) = 
        { defaults with
            Verbosity = Some(MSBuildVerbosity.Quiet)}

    MSBuild.build setParams "./GitHubVS.sln"
    *)
    ()
)

Target.create "Test" (fun _ ->
    ()
)

Target.create "Coverage" (fun _ ->
    ()
)

Target.create "Default" (fun _ -> 
    ()
)

open Fake.Core.TargetOperators
"Clean" ==> "Build"
"BumpVersion" =?> ("Build", runBumpVersion)

"Build" ==> "Test" ==> "Default"
"Build" ==> "Coverage" ==> "Default"

// start build
Target.runOrDefaultWithArguments "Default"
