using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

public static class RepositoryHelpers
{
    public static void UpdateSubmodules(Repository repo)
    {
        foreach (var submodule in repo.Submodules)
        {
            var subDir = Path.Combine(repo.Info.WorkingDirectory, submodule.Path);
            Directory.CreateDirectory(subDir); // Required to avoid NotFoundException
            repo.Submodules.Update(submodule.Name, new SubmoduleUpdateOptions { Init = true });
        }
    }

    public static void CommitFile(Repository repo, string path, string content, Signature author)
    {
        var contentFile = Path.Combine(repo.Info.WorkingDirectory, path);
        File.WriteAllText(contentFile, content);
        Commands.Stage(repo, path);
        repo.Commit("message", author, author);
    }

    public static void AddSubmodule(Repository repo, string name, string path, Repository subRepo)
    {
        var modulesPath = ".gitmodules";
        var modulesFile = Path.Combine(repo.Info.WorkingDirectory, modulesPath);
        if (!File.Exists(modulesFile))
        {
            File.WriteAllText(modulesFile, "");
        }

        var modulesConfig = Configuration.BuildFrom(modulesFile);
        modulesConfig.Set($"submodule.{name}.path", path, ConfigurationLevel.Local);
        modulesConfig.Set($"submodule.{name}.url", subRepo.Info.WorkingDirectory, ConfigurationLevel.Local);
        Commands.Stage(repo, modulesPath);

        AddGitLinkToTheIndex(repo.Index, path, subRepo.Head.Tip.Sha);
    }

    public static void AddGitLinkToTheIndex(Index index, string path, string sha)
    {
        var id = new ObjectId(sha);
        var mode = Mode.GitLink;
        index.GetType().InvokeMember("AddEntryToTheIndex", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod, null,
            index, new object[] { path, id, mode }, CultureInfo.InvariantCulture);
    }
}
