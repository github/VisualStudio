# Making changes to a pull request

When a topic branch is [checked out](review-a-pull-request-in-visual-studio.md), you can commit changes to it and push and pull like any other branch. If the pull request branch is located in a fork and was checked out from the Pull Request Details view in the GitHub pane, then a remote to that fork will be created automatically and the branch set to track the fork branch.

## Pulling changes to your local clone

If a Pull Request is checked out and the author adds new commits to the branch, then the option will be given to pull the changes locally. This works both for pull requests from the same repository and from a fork.

![](images/pr-pull-changes.png)

## Pushing changes

If you make commits locally to a topic branch, then you can push the changes to the remote branch. You can also do this from Git itself or from the Visual Studio Team Explorer **Sync** view.

> Note: for this to work with Pull Requests that come from forks, then you must be a maintainer on the repository and the Pull Request submitter must have checked [Allow edits from maintainers](https://help.github.com/articles/allowing-changes-to-a-pull-request-branch-created-from-a-fork/) when submitting the Pull Request.

If there are commits on the branch on the remote repository that you don't have on your local clone, you must pull them to your local clone and [resolve any conflicts](https://help.github.com/articles/addressing-merge-conflicts/) before you can push your local commits back to the remote repository.
