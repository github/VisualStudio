## Contributing

[fork]: https://github.com/github/VisualStudio/fork
[pr]: https://github.com/github/VisualStudio/compare
[code-of-conduct]: http://todogroup.org/opencodeofconduct/#VisualStudio/opensource@github.com
[readme]: https://github.com/github/VisualStudio#build

Hi there! We're thrilled that you'd like to contribute to this project. Your help is essential for keeping it great.

This project adheres to the [Open Code of Conduct][code-of-conduct]. By participating, you are expected to uphold this code.

## Submitting a pull request

0. [Fork][] and clone the repository (see Build Instructions in the [README][readme])
0. Create a new branch: `git checkout -b my-branch-name`
0. Make your change, add tests, and make sure the tests still pass
0. Push to your fork and [submit a pull request][pr]
0. Pat your self on the back and wait for your pull request to be reviewed and merged.

Here are a few things you can do that will increase the likelihood of your pull request being accepted:

- Follow the existing code's style.
- Write tests.
- Keep your change as focused as possible. If there are multiple changes you would like to make that are not dependent upon each other, consider submitting them as separate pull requests.
- Write a [good commit message](http://tbaggery.com/2008/04/19/a-note-about-git-commit-messages.html).

There are certain areas of the extension that are restricted in what they can do and what dependencies they can have.

- All sections and navigation items in the various Team Explorer areas are instantiated regardless of whether they're shown, so code in these areas needs to be as lazily run as possible.
- Anything in the Team Explorer Home page is extremely restricted in what dependencies it can have. Code in this area needs to be as fast and as minimal as possible.
- Team Explorer content outside the Home page is slightly less restricted, but not by much
- Dialogs and views that don't inherit from TeamExplorer classes are free to use what they need.

## Submitting an Issue

### Bug Reporting

Here are a few helpful tips when reporting a bug:
- Verify the bug resides in the GitHub for Visual Studio extension
  - A lot of functionality within this extension resides in the Team Explorer       pane, where there are additional tools to manage and collaborate on source code, including Visual Studio's Git Extension, which is owned by Microsoft.
  - If this bug not is related to the GitHub extension, visit the [Visual Studio support page](https://www.visualstudio.com/support/support-overview-vs) for help
- A log file can be helpful in diagnosing bug issues, if you'd like to include one in your issue, here is how you'd do it:
  0. Close Visual Studio if it's open
  0. Open a Developer Command Prompt for VS2015
  0. Run devenv /log
  0. Close VS
  0. Locate the following files on your system and email them to windows@github.com or copy their contents into the issue:
  - `%appdata%\Microsoft\VisualStudio\14.0\ActivityLog.xml`
  - `%localappdata%\temp\extension.log`
- Screenshots are also helpful in diagnosing bugs and understanding the state of the extension when it's experiencing problems.

### Feature Requests
If you have a feature that you think would be a great addition to the extension, we might already have thought about it too, so be sure to check if your suggestion matches our roadmap before making a request.

## Things to improve in the current version

- Localization

## Roadmap and future feature ideas

- Pull Requests
- Issues

## Resources

- [Contributing to Open Source on GitHub](https://guides.github.com/activities/contributing-to-open-source/)
- [Using Pull Requests](https://help.github.com/articles/using-pull-requests/)
- [GitHub Help](https://help.github.com)
