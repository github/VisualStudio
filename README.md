# GitHub Extension for Visual Studio

## Notices

### If you are having issues with the installer, please read

If you need to upgrade, downgrade, or uninstall the extension, and are having problems doing so, refer to this issue: https://github.com/github/VisualStudio/issues/1394 which details common problems and solutions when using the installer.

### The location of the submodules has changed as of 31-01-2017

If you have an existing clone, make sure to run `git submodule sync` to update your local clone with the new locations for the submodules.

## About

The GitHub Extension for Visual Studio provides GitHub integration in Visual Studio 2015 and newer.
Most of the extension UI lives in the Team Explorer pane, which is available from the View menu.

Official builds of this extension are available at [the official website](https://visualstudio.github.com).

[![Build status](https://ci.appveyor.com/api/projects/status/dl8is5iqwt9qf3t7/branch/master?svg=true)](https://ci.appveyor.com/project/github-windows/visualstudio/branch/master)
[![Build Status](https://github-editor-tools.visualstudio.com/VisualStudio/_apis/build/status/github.VisualStudio?branchName=master)](https://github-editor-tools.visualstudio.com/VisualStudio/_build/latest?definitionId=4&branchName=master)
[![Crowdin](https://d322cqt584bo4o.cloudfront.net/github-visual-studio/localized.svg)](https://crowdin.com/project/github-visual-studio)
[![codecov](https://codecov.io/gh/GitHub/VisualStudio/branch/master/graph/badge.svg)](https://codecov.io/gh/GitHub/VisualStudio)

[![Join the chat at freenode:github-vs](https://img.shields.io/badge/irc-freenode:%20%23github--vs-blue.svg)](http://webchat.freenode.net/?channels=%23github-vs) [![Join the chat at https://gitter.im/github/VisualStudio](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/github/VisualStudio?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

## Documentation
Visit the [documentation](https://github.com/github/VisualStudio/tree/master/docs) for details on how to use the features in the GitHub Extension for Visual Studio.

## Installing beta versions

Older and pre-release/beta/untested versions are available at [the releases page](https://github.com/github/VisualStudio/releases), and also via a custom gallery feed for Visual Studio.

You can configure the gallery by going to `Tools / Options / Extensions and Updates` and adding a new gallery with the url https://visualstudio.github.com/releases/feed.rss. The gallery will now be available from `Tools / Extensions and Updates`.

Beta releases will have `(beta)` in their title in the gallery, following the version number. You can view the release notes in the gallery by hovering over the description, or by clicking the `Release Notes` link on the right side.

## Build requirements

* Visual Studio 2017 (15.7.4)+
* Visual Studio SDK

## Build

Clone the repository and its submodules in a git GUI client or via the command line:

```txt
git clone https://github.com/github/VisualStudio
cd VisualStudio
git submodule init
git submodule deinit script
git submodule update
```

Open the `GitHubVS.sln` solution with Visual Studio 2017+.
To be able to use the GitHub API, you'll need to:

- [Register a new developer application](https://github.com/settings/developers) in your profile.
- Open [src/GitHub.Api/ApiClientConfiguration_User.cs](src/GitHub.Api/ApiClientConfiguration_User.cs) and fill out the clientId/clientSecret fields for your application. **Note this has recently changed location, so you may need to re-do this**

Build using Visual Studio 2017 or:

```txt
build.cmd
```

Install in live (non-Experimental) instances of Visual Studio 2015 and 2017:

```txt
install.cmd
```

Note, the script will only install in one instance of Visual Studio 2017 (Enterprise, Professional or Community).

## Build Flavors

The following can be executed via `cmd.exe`.

To build and install a `Debug` configuration VSIX:
```txt
build.cmd Debug
install.cmd Debug
```

To build and install a `Release` configuration VSIX:
```txt
build.cmd Release
install.cmd Release
```
## Logs
Logs can be viewed at the following location:

`%LOCALAPPDATA%\GitHubVisualStudio\extension.log`

## Troubleshooting

If you have issues building with failures similar to:

> "The type or namespace name does not exist..."

or

> "Unable to find project... Check that the project reference is valid and that the project file exists."*

Close Visual Studio and run the following command to update submodules and clean your environment.

```txt
clean.cmd
```

## More information
- Andreia Gaita's [presentation](https://www.youtube.com/watch?v=hz2hCO8e_8w) at Codemania 2016 about this extension.

## Contributing

Visit the [Contributor Guidelines](CONTRIBUTING.md) for details on how to contribute as well as the [Contributor Covenant Code of Conduct](CODE_OF_CONDUCT.md) for details on how to participate.

## Copyright

Copyright 2015 - 2018 GitHub, Inc.

Licensed under the [MIT License](LICENSE.md)
