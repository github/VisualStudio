using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using GitHub.InlineReviews.Models;
using Microsoft.VisualStudio.Text.Differencing;

namespace GitHub.InlineReviews.Services
{
    [Export(typeof(IDiffService))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class DiffService : IDiffService
    {
        static readonly Regex ChunkHeaderRegex = new Regex(@"^@@\s+\-(\d+),?\d+?\s+\+(\d+),?\d+?\s@@");
        static readonly char[] Newlines = new[] { '\r', '\n' };
        readonly ITextDifferencingService vsDiff;

        [ImportingConstructor]
        public DiffService(ITextDifferencingSelectorService diffSelector)
        {
            this.vsDiff = diffSelector.DefaultTextDifferencingService;
        }

        public IEnumerable<DiffChunk> Diff(string left, string right, int contextLines = 3)
        {
            var diff = vsDiff.DiffStrings(left, right, new StringDifferenceOptions
            {
                DifferenceType = StringDifferenceTypes.Line,
                IgnoreTrimWhiteSpace = true,
            });

            foreach (var difference in diff.Differences)
            {
                var chunk = new DiffChunk();

                for (var i = contextLines; i > 0; --i)
                {
                    if (difference.Left.Start - i < 0 || difference.Right.Start - i < 0) break;

                    chunk.Lines.Add(new DiffLine
                    {
                        OldLineNumber = (difference.Left.Start - i) + 1,
                        NewLineNumber = (difference.Right.Start - i) + 1,
                        Content = ' ' + diff.LeftSequence[difference.Left.Start - i].TrimEnd(Newlines),
                    });
                }

                if (difference.DifferenceType == DifferenceType.Remove || difference.DifferenceType == DifferenceType.Change)
                {
                    for (var i = 0; i < difference.Left.Length; ++i)
                    {
                        chunk.Lines.Add(new DiffLine
                        {
                            Type = DiffChangeType.Delete,
                            OldLineNumber = (difference.Left.Start + i) + 1,
                            Content = '-' + diff.LeftSequence[difference.Left.Start + i].TrimEnd(Newlines),
                        });
                    }
                }

                if (difference.DifferenceType == DifferenceType.Add || difference.DifferenceType == DifferenceType.Change)
                {
                    for (var i = 0; i < difference.Right.Length; ++i)
                    {
                        chunk.Lines.Add(new DiffLine
                        {
                            Type = DiffChangeType.Add,
                            NewLineNumber = (difference.Right.Start + i) + 1,
                            Content = '+' + diff.RightSequence[difference.Right.Start + i].TrimEnd(Newlines),
                        });
                    }
                }

                chunk.OldLineNumber = chunk.Lines.FirstOrDefault(x => x.OldLineNumber != -1)?.OldLineNumber ?? -1;
                chunk.NewLineNumber = chunk.Lines.FirstOrDefault(x => x.NewLineNumber != -1)?.NewLineNumber ?? -1;
                yield return chunk;
            }
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
