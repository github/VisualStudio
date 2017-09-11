# GitHub Extension for Visual Studio

## Notices

### VS 2017 v15.3 users, please read

If you need to downgrade or uninstall the extension, **do not use** ***Revert*** in Visual Studio 2017 15.3. Instead, manually uninstall the extension with the steps listed in https://github.com/github/VisualStudio/issues/1206#issuecomment-326558902

If you have a corrupted extension cache, steps for fixing it are in https://github.com/github/VisualStudio/issues/1206#issuecomment-326053090

The Visual Studio 2017 15.3 installer [has a bug](https://github.com/github/VisualStudio/issues/1206) that causes a corruption of the installed extensions data when you revert an installation of the extension (see also [this MS issue](https://developercommunity.visualstudio.com/content/problem/102178/error-installing-github-extension.html)). Until VS 2017 15.4 comes out, **do not use Revert in** ***Extensions and Updates***.

### The location of the submodules has changed as of 31-01-2017

If you have an existing clone, make sure to run `git submodule sync` to update your local clone with the new locations for the submodules.

## About

The GitHub Extension for Visual Studio provides GitHub integration in Visual Studio 2015.
Most of the extension UI lives in the Team Explorer pane, which is available from the View menu.

Official builds of this extension are available at [the official website](https://visualstudio.github.com).

[![Join the chat at freenode:github-vs](https://img.shields.io/badge/irc-freenode:%20%23github--vs-blue.svg)](http://webchat.freenode.net/?channels=%23github-vs) [![Join the chat at https://gitter.im/github/VisualStudio](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/github/VisualStudio?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

## Documentation
Visit the [documentation](https://github.com/github/VisualStudio/tree/master/docs) for details on how to use the features in the GitHub Extension for Visual Studio.

## Installing beta versions

Older and pre-release/beta/untested versions are available at [the releases page](https://github.com/github/VisualStudio/releases), and also via a custom gallery feed for Visual Studio.

You can configure the gallery by going to `Tools / Options / Extensions and Updates` and adding a new gallery with the url https://visualstudio.github.com/releases/feed.rss. The gallery will now be available from `Tools / Extensions and Updates`.

Beta releases will have `(beta)` in their title in the gallery, following the version number. You can view the release notes in the gallery by hovering over the description, or by clicking the `Release Notes` link on the right side.

## Build requirements

* Visual Studio 2015+
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

Visual Studio extensions have to be signed, so you need to create a signing key with the name `publickey.snk` for your build in the root of the repository:

```txt
sn -k `publickey.snk`
```

Open the `GitHubVS.sln` solution with Visual Studio 2015+.
To be able to use the GitHub API, you'll need to:

- [Register a new developer application](https://github.com/settings/developers) in your profile.
- Open [src/GitHub.Api/ApiClientConfiguration_User.cs](src/GitHub.Api/ApiClientConfiguration_User.cs) and fill out the clientId/clientSecret fields for your application. **Note this has recently changed location, so you may need to re-do this**

Build using Visual Studio 2015 or:

```txt
build.cmd
```

Install in live (non-Experimental) instances of Visual Studio 2015 and 2017:

```txt
install.cmd
```

Note, the script will only install in one instance of Visual Studio 2017 (Enterprise, Professional or Community).

## Build Flavors

By default, building will create a VSIX with `Experimental="true"` and `AllUsers="false"` in its `extension.vsixmanifest`. These settings are necessary in order to easily install a standalone VSIX file. There is no need to uninstall the version previously installed via Visual Studio setup / Extensions and Updates.

The following can be executed via `cmd.exe`.

To build and install a `Debug` configuration VSIX:
```txt
build.cmd
install.cmd
```

To build and install a `Release` configuration VSIX:
```txt
set Configuration=Release
build.cmd
install.cmd
```

To build a VSIX that can be installed via a gallery feed on Extensions and Updates:
```txt
set Configuration=Release
set IsExperimental=false
build.cmd
```

Note, attempting to install `IsExperimental=false` builds of the VSIX is not recommended.

## More information
- Andreia Gaita's [presentation](https://www.youtube.com/watch?v=hz2hCO8e_8w) at Codemania 2016 about this extension.

## Contributing

Visit the [Contributor Guidelines](CONTRIBUTING.md) for details on how to contribute as well as the [Open Code of Conduct](http://todogroup.org/opencodeofconduct/#VisualStudio/opensource@github.com) for details on how to participate.

## Copyright

Copyright 2015 - 2017 GitHub, Inc.

Licensed under the [MIT License](LICENSE.md)
