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

let powershell script =
    Shell.Exec("powershell.exe", "-NoProfile -ExecutionPolicy Bypass -File " + script)
    |> ignore

let isAppveyor = AppVeyor.detect()

let buildNumber =
    match (Environment.environVarOrNone "BuildNumber"), isAppveyor with
    | Some(x), _ -> Some(x)
    | None, true -> Some(AppVeyor.Environment.BuildNumber)
    | _ -> None

let bumpVersion =
    match buildNumber, (Environment.hasEnvironVar "Package") with
    | (Some(x), true) -> (int x) > -1
    | _ -> false        

Target.create "Clean" (fun _ ->
  ["build"]
  |> Seq.iter Directory.delete
)

Target.create "BumpVersion" (fun _ ->
    Trace.logfn "Bumping Version"
    powershell ("scripts\\Bump-Version.ps1 -BumpBuild -BuildNumber:" + buildNumber.Value)
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
"BumpVersion" =?> ("Build", bumpVersion)

"Build" ==> "Test" ==> "Default"
"Build" ==> "Coverage" ==> "Default"

// start build
Target.runOrDefaultWithArguments "Default"
