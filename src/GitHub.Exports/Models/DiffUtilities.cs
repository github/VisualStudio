using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Diagnostics.CodeAnalysis;

namespace GitHub.Models
{
    public static class DiffUtilities
    {
        const string NoNewLineAtEnd = "\\ No newline at end of file";
        static readonly Regex ChunkHeaderRegex = new Regex(@"^@@\s+\-(\d+),?\d+?\s+\+(\d+),?\d+?\s@@");

        public static IEnumerable<DiffChunk> ParseFragment(string diff)
        {
            using (var reader = new StringReader(diff))
            {
                string line;
                DiffChunk chunk = null;
                int diffLine = 0;
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
                    else if (line == NoNewLineAtEnd)
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

        public static DiffLine Match(IEnumerable<DiffChunk> diff, IList<DiffLine> target)
        {
            int j = 0;

            if (target.Count == 0)
            {
                return null; // no lines to match
            }

            foreach (var source in diff)
            {
                for (var i = source.Lines.Count - 1; i >= 0; --i)
                {
                    if (source.Lines[i].Content == target[j].Content)
                    {
                        if (++j == target.Count) return source.Lines[i + j - 1];
                    }
                    else
                    {
                        j = 0;
                    }
                }
            }

            return null;
        }

        [SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object)")]
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
