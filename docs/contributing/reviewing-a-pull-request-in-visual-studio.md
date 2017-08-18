# Reviewing a pull request in Visual Studio

GitHub for Visual Studio provides facilities for reviewing a pull request directly in Visual Studio.

1. Open a solution in a GitHub repository.

2. Open **Team Explorer** and click the **Pull Requests** button to open the **GitHub** pane.

   ![Pull Requests button in the Team Explorer pane](images/pull-requests-button.png)

3. Click the title of the pull request to be reviewed.

## Viewing a pull request

The Pull Request Details view shows the current state of the pull request, including information about who created the pull request, the source and target branch, and the files changed.

![The details of a single pull request in the GitHub pane](images/pr-details.png)

## Checking out a pull request

To check out the pull request branch, click the **Checkout [branch]** link where [branch] is the name of the branch that will be checked out.

![Location of the checkout link in the GitHub pull request details page](images/pr-details-checkout-link.png)

If the pull request is from a fork then a remote will be added to the forked repository and the branch checked out locally. This remote will automatically be cleaned up when the local branch is deleted.

> Note that you cannot check out a pull request branch when your working directory has uncommitted changes. First commit or stash your changes and then refresh the Pull Request view.

## Viewing Changes

To view the changes in the pull request for a file, double click a file in the **Changed Files** tree. This will open the Visual Studio diff viewer.

![Diff of two files in the Visual Studio diff viewer](images/pr-diff-files.png)

You can also right-click on a file in the changed files tree to get more options:

- **View Changes**: This is the default option that is also triggered when the file is double-clicked. It shows the changes to the file that are introduced by the pull request.
- **View File**: This opens a read-only editor showing the contents of the file in the pull request.
- **View Changes in Solution**: This menu item is only available when the pull request branch is checked out. It shows the changes in the pull request, but the right hand side of the diff is the file in the working directory. This view allows you to use Visual Studio navigation commands such as **Go to Definition (F12)**.
- **Open File in Solution**: This menu item opens the working directory file in an editor.

## Leaving Comments

You can add comments to a pull request directly from Visual Studio. When a file is [open in the diff viewer](#viewing-changes) you can click the **Add Comment** (TODO: Icon here) icon in the margin to add a comment on a line.

TODO: Screenshot

Existing comments left by you or other reviewers will also show up in this margin. Click the icon to open an inline conversation view from which you can review and reply to comments:



