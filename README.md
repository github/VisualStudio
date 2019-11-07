# GitHub Extension for Visual Studio

## About

The GitHub Extension for Visual Studio provides GitHub integration in Visual Studio 2015 and newer.
Most of the extension UI lives in the Team Explorer pane, which is available from the View menu.

Official builds of this extension are available at [the official website](https://visualstudio.github.com).

[![Build Status](https://github-editor-tools.visualstudio.com/VisualStudio/_apis/build/status/github.VisualStudio?branchName=master)](https://github-editor-tools.visualstudio.com/VisualStudio/_build/latest?definitionId=10&branchName=master)

[![Follow GitHub for Visual Studio](https://img.shields.io/twitter/follow/GitHubVS.svg?style=social "Follow GitHubVS")](https://twitter.com/githubvs?ref_src=twsrc%5Etfw) [![Join the chat at https://gitter.im/github/VisualStudio](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/github/VisualStudio?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

## Documentation
Visit the [documentation](https://github.com/github/VisualStudio/tree/master/docs) for details on how to use the features in the GitHub Extension for Visual Studio.

## Build requirements

* Visual Studio 2017 (15.7.4)+
* Visual Studio SDK
* The built VSIX will work with Visual Studio 2015 or newer

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
