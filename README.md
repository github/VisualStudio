# GitHub Extension for Visual Studio

## About

The GitHub Extension for Visual Studio provides GitHub integration in Visual Studio 2015 and newer.
Most of the extension UI lives in the Team Explorer pane, which is available from the View menu.

Official builds of this extension are available at the [Visual Studio Marketplace](https://marketplace.visualstudio.com/items?itemName=GitHub.GitHubExtensionforVisualStudio).

![CI](https://github.com/github/visualstudio/workflows/CI/badge.svg)

[![Follow GitHub for Visual Studio](https://img.shields.io/twitter/follow/GitHubVS.svg?style=social "Follow GitHubVS")](https://twitter.com/githubvs?ref_src=twsrc%5Etfw) [![Join the chat at https://gitter.im/github/VisualStudio](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/github/VisualStudio?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

## Documentation
Visit the [documentation](https://github.com/github/VisualStudio/tree/master/docs) for details on how to use the features in the GitHub Extension for Visual Studio.

## Build requirements

* Visual Studio 2019
  * `.NET desktop development` workload
  * `.NET Core cross platform development` workload
  * `Visual Studio extension development` workload

The built VSIX will work with Visual Studio 2015 or newer

## Build

Clone the repository and its submodules.

To be able to use the GitHub API, you'll need to:

- [Register a new developer application](https://github.com/settings/developers) in your profile
- Create an environment variable `GitHubVS_ClientID` with your `Client ID`
- Create an environment variable `GitHubVS_ClientSecret` with your `Client Secret`

Execute `build.cmd`

## Visual Studio Build

Build `GitHubVS.sln` using Visual Studio 2019.

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

Copyright 2015 - 2019 GitHub, Inc.

Licensed under the [MIT License](LICENSE.md)
