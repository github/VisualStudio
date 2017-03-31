# Reviewing a pull request in Visual Studio

GitHub for Visual Studio provides facilities for reviewing a pull request directly in Visual Studio.

1. Open a solution in a GitHub repository.

2. Open **Team Explorer** and click the **Pull Requests** button to open the **GitHub** pane.

   ![image](images/pull-requests-button.png)

3. Click the title of the pull request to be reviewed.

## Viewing a pull request

The Pull Request Details view shows the current state of the pull request, including information about who created the pull request, the source and target branch, and the files changed.

![image](images/pr-details.png)

## Checking out a pull request

To check out the pull request branch, click the **Checkout [branch]** link where [branch] is the name of the branch that will be checked out.

![image](images/pr-details-checkout-link.png)

If the pull request is from a fork then a remote will be added to the forked repository and the branch checked out locally. This remote will automatically be cleaned up when the local branch is deleted.

> Note that you cannot check out a pull request branch when your working directory has uncommitted changes. First commit or stash your changes and then refresh the Pull Request view.

## Comparing files

To compare the contents of a file in the pull request with its content on the target branch, double click a file in the **Changed Files** tree. This will open the Visual Studio diff viewer. If the pull request has been checked out, the right hand pane will be editable.

![image](images/pr-diff-files.png)

If the pull request is checked out, right clicking on a file on the **Changed Files** tree and selecting **Open File** will open the file for editing in Visual Studio.
