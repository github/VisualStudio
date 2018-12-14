using Octokit;
using System;
using System.IO;
using System.IO.Compression;

/// <summary>
/// This base class will get its methods called by the most-derived
/// classes. The calls are injected by the EntryExitMethodDecorator Fody
/// addin, so don't be surprised if you don't see any calls in the code.
/// </summary>
public class TestBaseClass
{

    protected static User CreateOctokitUser(string login = "login", string url = "https://url")
    {
        return new User("https://url", "bio", "blog", 1, "GitHub",
            DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, 0, "email", 100, 100, true, url,
            10, 42, "location", login, "name", null, 0, new Plan(),
            1, 1, 1, "https://url", new RepositoryPermissions(true, true, true),
            false, null, null);
    }

    protected static Organization CreateOctokitOrganization(string login)
    {
        return new Organization("https://url", "", "", 1, "GitHub", DateTimeOffset.UtcNow, 0, "email", 100, 100, true, "http://url", 10, 42, null, "somewhere", login, "Who cares", 1, new Plan(), 1, 1, 1, "https://url", "billing");
    }

    protected static Repository CreateRepository(string owner, string name, string domain = "github.com", long id = 1, Repository parent = null)
    {
        var cloneUrl = "https://" + domain + "/" + owner + "/" + name;
        string notCloneUrl = cloneUrl + "-x";
        return new Repository(notCloneUrl, notCloneUrl, cloneUrl, notCloneUrl, notCloneUrl, notCloneUrl, notCloneUrl,
            id, null, CreateOctokitUser(owner),
            name, "fullname", "description", notCloneUrl, "c#", false, parent != null, 0, 0, "master",
            0, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow,
            new RepositoryPermissions(), parent, null, null, true, false, false, false, 0, 0, null, null, null, false);
    }

    protected static PullRequest CreatePullRequest(User user, int id, ItemState state, string title,
        DateTimeOffset createdAt, DateTimeOffset updatedAt, int commentCount = 0)
    {
        var uri = new Uri("https://url");
        var uris = uri.ToString();
        var repo = new Repository(uris, uris, uris, uris, uris, uris, uris,
            1, null, user, "Repo", "Repo", string.Empty, string.Empty, string.Empty,
            false, false, 0, 0, "master",
            0, null, createdAt, updatedAt,
            null, null, null, null,
            false, false, false,
            false, 0, 0,
            null, null, null, false);
        return new PullRequest(0, null, uris, uris, uris, uris, uris, uris,
            id, state, title, "", createdAt, updatedAt,
            null, null,
            new GitReference(null, uri.ToString(), "foo:bar", "bar", "123", user, repo),
            new GitReference(null, uri.ToString(), "foo:baz", "baz", "123", user, repo),
            user, null, null, false, null, null, null,
            commentCount, 0, 0, 0, 0,
            null, false, null, null);
    }

    protected class TempDirectory : IDisposable
    {
        public TempDirectory()
        {
            var f = Path.GetTempFileName();
            var name = Path.GetFileNameWithoutExtension(f);
            File.Delete(f);
            Directory = new DirectoryInfo(Path.Combine(Path.GetTempPath(), name));
            Directory.Create();
        }

        public DirectoryInfo Directory { get; }

        public void Dispose()
        {
            // Remove any read-only attributes
            SetFileAttributes(Directory, FileAttributes.Normal);
            Directory.Delete(true);
        }

        static void SetFileAttributes(DirectoryInfo dir, FileAttributes attributes)
        {
            foreach (DirectoryInfo subdir in dir.GetDirectories())
            {
                SetFileAttributes(subdir, attributes);
            }

            foreach (var file in dir.GetFiles())
            {
                File.SetAttributes(file.FullName, attributes);
            }
        }
    }

    protected class TempRepository : TempDirectory
    {
        public TempRepository(string name, byte[] repositoryZip)
            : base()
        {
            var outputZip = Path.Combine(Directory.FullName, name + ".zip");
            var outputDir = Path.Combine(Directory.FullName, name);
            var repositoryPath = Path.Combine(outputDir, name);
            File.WriteAllBytes(outputZip, repositoryZip);
            ZipFile.ExtractToDirectory(outputZip, outputDir);
            Repository = new LibGit2Sharp.Repository(repositoryPath);
        }

        public LibGit2Sharp.Repository Repository
        {
            get;
        }
    }
}
