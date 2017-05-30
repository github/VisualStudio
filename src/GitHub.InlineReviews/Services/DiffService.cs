using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GitHub.Models;
using GitHub.Services;
using LibGit2Sharp;

namespace GitHub.InlineReviews.Services
{
    [Export(typeof(IDiffService))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class DiffService : IDiffService
    {
        static readonly Regex ChunkHeaderRegex = new Regex(@"^@@\s+\-(\d+),?\d+?\s+\+(\d+),?\d+?\s@@");
        static readonly char[] Newlines = new[] { '\r', '\n' };
        readonly IGitClient gitClient;

        [ImportingConstructor]
        public DiffService(IGitClient gitClient)
        {
            this.gitClient = gitClient;
        }

        public async Task<IList<DiffChunk>> Diff(
            IRepository repo,
            string sha,
            string path,
            byte[] contents)
        {
            var changes = await gitClient.CompareWith(repo, sha, path, contents);
            return ParseFragment(changes.Patch).ToList();
        }

        public IEnumerable<DiffChunk> ParseFragment(string diff)
        {
            using (var reader = new StringReader(diff))
            {
                string line;
                DiffChunk chunk = null;
                int diffLine = 1;
                int oldLine = -1;
                int newLine = -1;

                while ((line = reader.ReadLine()) != null)
                {
                    var headerMatch = ChunkHeaderRegex.Match(line);

                    if (headerMatch.Success)
                    {
                        if (chunk != null)
                        {
                            yield return chunk;
                        }

                        chunk = new DiffChunk
                        {
                            OldLineNumber = oldLine = int.Parse(headerMatch.Groups[1].Value),
                            NewLineNumber = newLine = int.Parse(headerMatch.Groups[2].Value),
                            DiffLine = diffLine,
                        };
                    }
                    else if (line == "\\ No newline at end of file")
                    {
                        break;
                    }
                    else if (chunk != null)
                    {
                        var type = GetLineChange(line[0]);

                        chunk.Lines.Add(new DiffLine
                        {
                            Type = type,
                            OldLineNumber = type != DiffChangeType.Add ? oldLine : -1,
                            NewLineNumber = type != DiffChangeType.Delete ? newLine : -1,
                            DiffLineNumber = diffLine,
                            Content = line,
                        });

                        switch (type)
                        {
                            case DiffChangeType.None:
                                ++oldLine;
                                ++newLine;
                                break;
                            case DiffChangeType.Delete:
                                ++oldLine;
                                break;
                            case DiffChangeType.Add:
                                ++newLine;
                                break;
                        }
                    }

                    ++diffLine;
                }

                if (chunk != null)
                {
                    yield return chunk;
                }
            }
        }

        static DiffChangeType GetLineChange(char c)
        {
            switch (c)
            {
                case ' ': return DiffChangeType.None;
                case '+': return DiffChangeType.Add;
                case '-': return DiffChangeType.Delete;
                default: throw new InvalidDataException($"Invalid diff line change char: '{c}'.");
            }
        }
    }
}
