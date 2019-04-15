#r "paket: groupref FakeBuild //"
#load "./.fake/build.fsx/intellisense.fsx"

open Fake.IO
open Fake.BuildServer
open Fake.IO.Globbing.Operators
open Fake.Core
open Fake.Tools

BuildServer.install [
    AppVeyor.Installer
]

let isAppveyor = AppVeyor.detect()

Target.create "Clean" (fun _ ->
  ()
)

Target.create "Build" (fun _ ->
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

"Build" ==> "Test" ==> "Default"
"Build" ==> "Coverage" ==> "Default"

// start build
Target.runOrDefault "Default"
