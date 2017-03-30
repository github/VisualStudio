# Review a Pull Request in Visual Studio

GitHub for Visual Studio provides facilities for reviewing a Pull Request directly in Visual Studio.

1. Open a solution in a GitHub repository.

2. Open **Team Explorer** and click the **Pull Requests** button to open the **GitHub** pane.

   ![image](images/pull-requests-button.png)

3. Click the title of the Pull Request to be reviewed.


## Viewing a Pull Request

The Pull Request details view shows the current state of the Pull Request, including information about who created the pull request, the source and target branch, and the files changed.

![image](images/pr-details.png)

## Checking out a Pull Request

To check out the Pull Request branch, click the **Checkout [branch]** link where [branch] is the name of the branch that will be checked out.

![image](images/pr-details-checkout-link.png)

If the Pull Request is from a fork then a remote will be added to the forked repository and the branch checked out locally. This remote will automatically be cleaned up when the local branch is deleted. 

> Note that you cannot check out a pull request branch when your working directory has uncommitted changes. First commit or stash your changes and then refresh the Pull Request view.

## Comparing Files

To compare the contents of a file in the Pull Request with its content on the target branch, double click a file in the **Changed Files** tree. This will open the Visual Studio diff viewer.

![image](images/pr-diff-files.png)

If the Pull Request is checked out, right clicking on a file on the **Changed Files** tree and selecting **Open File** will open the file for editing in Visual Studio.
