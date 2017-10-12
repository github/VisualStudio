using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace GitHub.Models
{
    public static class DiffUtilities
    {
        static readonly Regex ChunkHeaderRegex = new Regex(@"^@@\s+\-(\d+),?\d*\s+\+(\d+),?\d*\s@@");

        public static IEnumerable<DiffChunk> ParseFragment(string diff)
        {
            diff = NormalizeLineEndings(diff);

            // Optimize for common case where there are no loose carriage returns.
            var hasCarriageReturn = HasCarriageReturn(diff);
            if (hasCarriageReturn)
            {
                diff = EscapeCarriageReturns(diff);
            }

            using (var reader = new StringReader(diff))
            {
                string line;
                DiffChunk chunk = null;
                int diffLine = -1;
                int oldLine = -1;
                int newLine = -1;

                while ((line = reader.ReadLine()) != null)
                {
                    if (hasCarriageReturn)
                    {
                        line = UnescapeCarriageReturns(line);
                    }

                    var headerMatch = ChunkHeaderRegex.Match(line);

                    if (headerMatch.Success)
                    {
                        if (chunk != null)
                        {
                            yield return chunk;
                        }

                        if (diffLine == -1) diffLine = 0;

                        chunk = new DiffChunk
                        {
                            OldLineNumber = oldLine = int.Parse(headerMatch.Groups[1].Value),
                            NewLineNumber = newLine = int.Parse(headerMatch.Groups[2].Value),
                            DiffLine = diffLine,
                        };
                    }
                    else if (chunk != null)
                    {
                        var type = GetLineChange(line[0]);

                        // This might contain info about previous line (e.g. "\ No newline at end of file").
                        if (type != DiffChangeType.Control)
                        {
                            chunk.Lines.Add(new DiffLine
                            {
                                Type = type,
                                OldLineNumber = type != DiffChangeType.Add ? oldLine : -1,
                                NewLineNumber = type != DiffChangeType.Delete ? newLine : -1,
                                DiffLineNumber = diffLine,
                                Content = line,
                            });

                            var lineCount = 1;
                            if (hasCarriageReturn)
                            {
                                lineCount += CountCarriageReturns(line);
                            }

                            switch (type)
                            {
                                case DiffChangeType.None:
                                    oldLine += lineCount;
                                    newLine += lineCount;
                                    break;
                                case DiffChangeType.Delete:
                                    oldLine += lineCount;
                                    break;
                                case DiffChangeType.Add:
                                    newLine += lineCount;
                                    break;
                            }
                        }
                    }

                    if (diffLine != -1) ++diffLine;
                }

                if (chunk != null)
                {
                    yield return chunk;
                }
            }
        }

        public static DiffLine Match(IEnumerable<DiffChunk> diff, IList<DiffLine> target)
        {
            if (target.Count == 0)
            {
                return null; // no lines to match
            }

            foreach (var source in diff)
            {
                var matches = 0;
                for (var i = source.Lines.Count - 1; i >= 0; --i)
                {
                    if (source.Lines[i].Content == target[matches].Content)
                    {
                        matches++;
                        if (matches == target.Count || i == 0)
                        {
                            return source.Lines[i + matches - 1];
                        }
                    }
                    else
                    {
                        i += matches;
                        matches = 0;
                    }
                }
            }

            return null;
        }

        static string NormalizeLineEndings(string text)
        {
            return text.Replace("\r\n", "\n");
        }

        static bool HasCarriageReturn(string text)
        {
            return text.IndexOf('\r') != -1;
        }

        static string EscapeCarriageReturns(string text)
        {
            return text.Replace("\r", "\\r");
        }

        static string UnescapeCarriageReturns(string text)
        {
            return text.Replace("\\r", "\r");
        }

        static int CountCarriageReturns(string text)
        {
            int count = 0;
            foreach (var ch in text)
            {
                if (ch == '\r') count++;
            }

            return count;
        }

        [SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object)")]
        static DiffChangeType GetLineChange(char c)
        {
            switch (c)
            {
                case ' ': return DiffChangeType.None;
                case '+': return DiffChangeType.Add;
                case '-': return DiffChangeType.Delete;
                case '\\': return DiffChangeType.Control;
                default: throw new InvalidDataException($"Invalid diff line change char: '{c}'.");
            }
        }
    }
}
