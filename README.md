# GitHub for Visual Studio Extension

## Background

GitHub and Microsoft are developing a joint extension to Visual Studio 2015 to be [launched at BUILD](http://www.buildwindows.com/) in the end of April. This extension will be included as part of Visual Studio.

Over time, we'd like to open source the extension and develop it in public, but for this initial release, we have a tight schedule and are constrained in what we can do. 

## Prerequisites

Most dependencies are managed via [nuget](http://nuget.org/).

__Make sure you have the following installed!__

* Visual Studio 2015 preview bits

## Debugging

You can run and debug in the experimental instance of Visual Studio by doing this:

1. In the properties of the GitHub project go to the debug tab.
2. Select **Start external program** and set it to devenv.exe:

`C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\devenv.exe`

3. Add `/RootSuffix Exp` to the **command line arguments**.
4. Press **F5** to debug.
