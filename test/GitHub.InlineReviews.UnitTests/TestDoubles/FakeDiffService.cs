using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GitHub.InlineReviews.Services;
using GitHub.Models;
using GitHub.Services;
using LibGit2Sharp;
using NSubstitute;

namespace GitHub.InlineReviews.UnitTests.TestDoubles
{
    sealed class FakeDiffService : IDiffService, IDisposable
    {
        readonly IRepository repository;
        readonly IDiffService inner;
        readonly Dictionary<string, string> commitAliases = new Dictionary<string, string>();

        public FakeDiffService()
        {
            this.repository = CreateRepository();
            this.inner = new DiffService(Substitute.For<IGitClient>());
        }

        public FakeDiffService(string path, string contents)
        {
            this.repository = CreateRepository();
            this.inner = new DiffService(Substitute.For<IGitClient>());
            AddFile(path, contents);
        }

        public string AddFile(string path, string contents)
        {
            var signature = new Signature("user", "user@user", DateTimeOffset.Now);
            var fullPath = Path.Combine(repository.Info.WorkingDirectory, path);
            var directory = Path.GetDirectoryName(fullPath);
            Directory.CreateDirectory(directory);
            File.WriteAllText(fullPath, contents);
            repository.Stage(path);
            repository.Commit("Added " + path, signature, signature);
            return repository.Head.Tip.Sha;
        }

        public string AddFile(string path, string contents, string commitAlias)
        {
            var sha = AddFile(path, contents);
            commitAliases.Add(commitAlias, sha);
            return sha;
        }

        public void Dispose()
        {
            var path = repository.Info.WorkingDirectory;

            // NOTE: IDiffService doesn't own the Repository object
            //repository.Dispose();

            // The .git folder has some files marked as readonly, meaning that a simple
            // Directory.Delete doesn't work here.
            DeleteDirectory(path);
        }

        public Task<IReadOnlyList<DiffChunk>> Diff(IRepository repo, string baseSha, string headSha, string path)
        {
            var blob1 = GetBlob(path, baseSha);
            var blob2 = GetBlob(path, headSha);
            var patch = repository.Diff.Compare(blob1, blob2).Patch;
            return Task.FromResult<IReadOnlyList<DiffChunk>>(DiffUtilities.ParseFragment(patch).ToList());
        }

        public Task<IReadOnlyList<DiffChunk>> Diff(string path, string baseSha, byte[] contents)
        {
            var tip = repository.Head.Tip.Sha;
            var stream = contents != null ? new MemoryStream(contents) : new MemoryStream();
            var blob1 = GetBlob(path, baseSha);
            var blob2 = repository.ObjectDatabase.CreateBlob(stream, path);
            var patch = repository.Diff.Compare(blob1, blob2).Patch;
            return Task.FromResult<IReadOnlyList<DiffChunk>>(DiffUtilities.ParseFragment(patch).ToList());
        }

        public Task<IReadOnlyList<DiffChunk>> Diff(string path, string contents)
        {
            return Diff(path, repository.Head.Tip.Sha, Encoding.UTF8.GetBytes(contents));
        }

        public Task<IReadOnlyList<DiffChunk>> Diff(IRepository repo, string baseSha, string headSha, string path, byte[] contents)
        {
            return Diff(path, baseSha, contents);
        }

        Blob GetBlob(string path, string id)
        {
            string sha;
            if (!commitAliases.TryGetValue(id, out sha)) sha = id;
            var commit = repository.Lookup<Commit>(sha);
            return commit?[path]?.Target as Blob;
        }

        static IRepository CreateRepository()
        {
            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempPath);
            Repository.Init(tempPath);

            var result = new Repository(tempPath);
            var signature = new Signature("user", "user@user", DateTimeOffset.Now);

            File.WriteAllText(Path.Combine(tempPath, ".gitattributes"), "* text=auto");
            result.Stage("*");
            result.Commit("Initial commit", signature, signature);

            return result;
        }

        static void DeleteDirectory(string path)
        {
            foreach (var d in Directory.EnumerateDirectories(path))
            {
                DeleteDirectory(d);
            }

            foreach (var f in Directory.EnumerateFiles(path))
            {
                var fileInfo = new FileInfo(f);
                fileInfo.Attributes = FileAttributes.Normal;
                fileInfo.Delete();
            }
        }
    }
}
