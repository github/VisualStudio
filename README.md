# GitHub Extension for Visual Studio

[![Join the chat at https://gitter.im/github/VisualStudio](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/github/VisualStudio?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

The GitHub Extension for Visual Studio provides GitHub integration in Visual Studio 2015.
Most of the extension UI lives in the Team Explorer pane, which is available from the View menu.

Official builds of this extension are available at [the official website](https://visualstudio.github.com).

## Build requirements

* Visual Studio 2015
* Visual Studio SDK

## Build

Clone the repository and its submodules in a git GUI client or via the command line:

```
git clone https://github.com/github/VisualStudio
cd VisualStudio
git submodule update --init
```

Open the `GitHubVS.sln` solution with Visual Studio 2015.
To be able to use the GitHub API, you'll need to:

- [Register a new developer application](https://github.com/settings/developers) in your profile.
- Open [src/GitHub.App/Api/ApiClientConfiguration.cs](src/GitHub.App/Api/ApiClientConfiguration.cs) and fill out the clientId/clientSecret fields for your application.

## Contributing

Visit the [Contributor Guidelines](CONTRIBUTING.md) for details on how to contribute as well as the [Open Code of Conduct](http://todogroup.org/opencodeofconduct/#VisualStudio/opensource@github.com) for details on how to participate.

## Copyright

Copyright 2015 GitHub, Inc.

Licensed under the [MIT License](LICENSE.md)
