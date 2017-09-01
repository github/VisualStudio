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

        public void AddFile(string path, string contents)
        {
            var signature = new Signature("user", "user@user", DateTimeOffset.Now);
            File.WriteAllText(Path.Combine(repository.Info.WorkingDirectory, path), contents);
            repository.Stage(path);
            repository.Commit("Added " + path, signature, signature);

            var tip = repository.Head.Tip.Sha;
        }

        public void Dispose()
        {
            var path = repository.Info.WorkingDirectory;
            repository.Dispose();

            // The .git folder has some files marked as readonly, meaning that a simple
            // Directory.Delete doesn't work here.
            DeleteDirectory(path);
        }

        public Task<IList<DiffChunk>> Diff(string path, byte[] contents)
        {
            var tip = repository.Head.Tip.Sha;
            var stream = contents != null ? new MemoryStream(contents) : new MemoryStream();
            var blob1 = repository.Head.Tip[path]?.Target as Blob;
            var blob2 = repository.ObjectDatabase.CreateBlob(stream, path);
            var patch = repository.Diff.Compare(blob1, blob2).Patch;
            return Task.FromResult<IList<DiffChunk>>(DiffUtilities.ParseFragment(patch).ToList());
        }

        public Task<IList<DiffChunk>> Diff(string path, string contents)
        {
            return Diff(path, Encoding.UTF8.GetBytes(contents));
        }

        public Task<IList<DiffChunk>> Diff(IRepository repo, string baseSha, string headSha, string path, byte[] contents)
        {
            return Diff(path, contents);
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
