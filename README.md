# GitHub for Visual Studio Extension

## Background

GitHub and Microsoft are developing a joint extension to Visual Studio 2015 to be [launched at BUILD](http://www.buildwindows.com/) in the end of April. This extension will be included as part of Visual Studio.

Over time, we'd like to open source the extension and develop it in public, but for this initial release, we have a tight schedule and are constrained in what we can do. 

## Project Members
Microsoft - (githubvscrew@microsoft.com)
* PM - Anthony Cangialosi (anthc@microsoft.com)
* PM - Cathy Sullivan (cathys@microsoft.com)
* Design - Ron Di Sandro (rdisan@exchange.microsoft.com)
* Dev - Art Leonard (artl@microsoft.com)
* Dev - Praveen Sethuraman (prasethu@microsoft.com)
* Dev - Jeff Robison (jeffro@microsoft.com)

GitHub
* Phil Haack (phil@github.com)
* Andreia Gaita (shana@github.com)

## Schedule

* March 18 8:00 PM - is when the last change to VS extensibility can happen. (3 weeks of ask mode, 2 weeks of escrow after that)
* April ?? When the last change to the extension itself can occur. We need to factor testing and stabilization time.
* April 29 is Build
* [Sprint Calendar](https://microsoft.sharepoint.com/teams/DD_VSIDE_GitHub/_layouts/15/WopiFrame.aspx?sourcedoc={CCE534D8-D6A7-4672-878C-C34CECB5F383}&file=GitHub%20for%20Visual%20Studio%20Timeline.xlsx&action=default) 

## Documents and Assets
https://microsoft.sharepoint.com/teams/DD_VSIDE_GitHub

Contact cathys@microsoft.com or anthc@microsoft.com for access to the site.

## Prerequisites for running the extension

* Visual Studio 2015 RC (ping The Desktop Room)

## Prerequisites for building

Most dependencies are managed via [nuget](http://nuget.org/).

__Make sure you have the following installed!__

* Visual Studio 2015 RC (githubbers, ping The Desktop Room)
* Microsoft Visual Studio 2015 SDK (githubbers, ping The Desktop Room)
* Optional for building the msi installer (not needed for the vsix, if you're not deploying to production you don't need this): [WiX toolset v3.10.0.1519](http://static.wixtoolset.org/releases/v3.10.0.1519/wix310.exe).
  * If you don't install this the MsiInstaller project will not load. This is fine.

## Building

* The only solution you should be opening is the GitHubVS.sln solution at the top.
* Either build the whole solution or build the GitHub.VisualStudio project, which will grab and build all dependencies
* If the build fails the first time, try again
  * If it still fails try building the Akavache projects individually (in Submodules/Akavache) and then the GitHub.VisualStudio project or the whole solution

## Debugging

You can run and debug in the experimental instance of Visual Studio by doing this:

1. In the properties of the GitHub project go to the debug tab.
2. Select **Start external program** and set it to devenv.exe:

`C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\devenv.exe`

3. Add `/RootSuffix Exp` to the **command line arguments**.
4. Press **F5** to debug.
