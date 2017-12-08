using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

    public static void AddSubmodule(Repository repo, string name, string path, string urlOrRelativePath)
    {
        var arguments = $"submodule add --name {name} {urlOrRelativePath} {path}";
        Execute("git", arguments, repo.Info.WorkingDirectory, Console.WriteLine);
    }

    static void Execute(string command, string arguments, string workingDir, Action<string> progress = null)
    {
        var startInfo = new ProcessStartInfo(command, arguments)
        {
            WorkingDirectory = workingDir,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true
        };

        using (var process = Process.Start(startInfo))
        {
            var outputReader = process.StandardOutput;

            string line;
            while ((line = outputReader.ReadLine()) != null)
            {
                progress?.Invoke(line);
            }
        }
    }

    public static void AddGitLinkToTheIndex(Index index, string path, string sha)
    {
        var id = new ObjectId(sha);
        var mode = Mode.GitLink;
        index.GetType().InvokeMember("AddEntryToTheIndex", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod, null,
            index, new object[] { path, id, mode });
    }
}
