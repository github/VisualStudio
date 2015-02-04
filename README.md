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

* March 16 (more or less) is when the last change to VS extensibility can happen.
* April ?? When the last change to the extension itself can occur. We need to factor testing and stabilization time.
* April 29 is Build

## Documents and Assets
https://microsoft.sharepoint.com/teams/DD_VSIDE_GitHub/_layouts/15/start.aspx#/SitePages/Home.aspx

Contact cathys@microsoft.com for access to the site.

## Prerequisites

Most dependencies are managed via [nuget](http://nuget.org/).

__Make sure you have the following installed!__

* Visual Studio 2015 preview bits
* [Microsoft Visual Studio 2015 Preview SDK](http://www.microsoft.com/en-us/download/details.aspx?id=44932)

## Debugging

You can run and debug in the experimental instance of Visual Studio by doing this:

1. In the properties of the GitHub project go to the debug tab.
2. Select **Start external program** and set it to devenv.exe:

`C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\devenv.exe`

3. Add `/RootSuffix Exp` to the **command line arguments**.
4. Press **F5** to debug.
