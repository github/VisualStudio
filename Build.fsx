#r "paket: groupref FakeBuild //"
#load "./.fake/build.fsx/intellisense.fsx"

open Fake.IO
open Fake.BuildServer
open Fake.IO.Globbing.Operators
open Fake.Core
open Fake.Core.TargetOperators
open Fake.DotNet

BuildServer.install [
    AppVeyor.Installer
]

let powershell script =
    Shell.Exec("powershell.exe", "-NoProfile -ExecutionPolicy Bypass -File " + script)
    |> ignore

let isAppveyor = AppVeyor.detect()

let fullBuild = Environment.hasEnvironVar "GHFVS_KEY"
Trace.logfn "%s Build" (if fullBuild then "Full" else "Partial")

let forVsInstaller, forPackage =
    match (Environment.environVarOrNone "BUILD_TYPE") with
    | Some("vsinstaller") -> true, false
    | Some("package") -> false, true
    | _ -> false, false

let package = fullBuild && forPackage
if package then
    Trace.logfn "Packaging"

let buildNumber =
    match (Environment.environVarOrNone "BuildNumber"), isAppveyor with
    | Some(x), _ -> Some(int x)
    | None, true -> Some(int AppVeyor.Environment.BuildNumber)
    | _ -> None

let bumpVersion =
    match buildNumber, package with
    | (Some(x), true) when x > -1 -> true
    | _ -> false        

Target.create "Clean" (fun _ ->
  ["build"]
  |> Seq.iter Directory.delete
)

Target.create "BumpVersion" (fun _ ->
    Trace.logfn "Bumping Version"
    powershell (sprintf "scripts\\Bump-Version.ps1 -BumpBuild -BuildNumber:%i" buildNumber.Value)
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

Target.create "Test" ignore

Target.create "Coverage" ignore

Target.create "Package" ignore

Target.create "Default" ignore

"Clean" ==> "BumpVersion"
"Clean" ==> "Build"
"BumpVersion" =?> ("Build", bumpVersion)
"Build" ==> "Test"
"Build" ==> "Coverage"
"Build" ==> "Package"

if package then
    "Default" <== [ "Test" ; "Coverage"; "Package" ]
else
    "Default" <== [ "Test" ; "Coverage" ]

// start build
Target.runOrDefault "Default"