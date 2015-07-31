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

## Things to improve in the current version

- Localization

## Roadmap and future feature ideas

- Pull Requests
- Issues

## Resources

- [Contributing to Open Source on GitHub](https://guides.github.com/activities/contributing-to-open-source/)
- [Using Pull Requests](https://help.github.com/articles/using-pull-requests/)
- [GitHub Help](https://help.github.com)
