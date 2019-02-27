using System;
using System.IO;
using System.Globalization;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Diagnostics.CodeAnalysis;
using GitHub.Extensions;
using System.Linq;

#pragma warning disable CA1034 // Nested types should not be visible

#pragma warning disable CA1034 // Nested types should not be visible

namespace GitHub.Models
{
    public static class DiffUtilities
    {
        static readonly Regex ChunkHeaderRegex = new Regex(@"^@@\s+\-(\d+),?\d*\s+\+(\d+),?\d*\s@@");

        public static IEnumerable<DiffChunk> ParseFragment(string diff)
        {
            Guard.ArgumentNotNull(diff, nameof(diff));

            var reader = new LineReader(diff);
            string line;
            DiffChunk chunk = null;
            int diffLine = -1;
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

                    if (diffLine == -1) diffLine = 0;

                    chunk = new DiffChunk
                    {
                        OldLineNumber = oldLine = int.Parse(headerMatch.Groups[1].Value, CultureInfo.InvariantCulture),
                        NewLineNumber = newLine = int.Parse(headerMatch.Groups[2].Value, CultureInfo.InvariantCulture),
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
                            //OldLineNumber = type != DiffChangeType.Add ? oldLine : -1,
                            //NewLineNumber = type != DiffChangeType.Delete ? newLine : -1,
                            OldLineNumber = oldLine,
                            NewLineNumber = newLine,
                            DiffLineNumber = diffLine,
                            Content = line,
                        });

                        var lineCount = 1;
                        lineCount += LineReader.CountCarriageReturns(line);

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

        public static DiffLine Match(IEnumerable<DiffChunk> diff, IList<DiffLine> target)
        {
            var diffLine = MatchLine(diff, target);
            if (diffLine != null)
            {
                return diffLine;
            }

            diffLine = MatchLineIgnoreComments(diff, target);
            if (diffLine != null)
            {
                return diffLine;
            }

            diffLine = MatchChunk(diff, target);
            if (diffLine != null)
            {
                return diffLine;
            }

            return null;
        }

        public static DiffLine MatchChunk(IEnumerable<DiffChunk> diff, IList<DiffLine> target)
        {
            if (target.Count == 0)
            {
                return null; // no lines to match
            }

            int j = 0;
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

        public static DiffLine MatchLine(IEnumerable<DiffChunk> chunks1, IList<DiffLine> targetLines)
        {
            var targetLine = targetLines.FirstOrDefault();
            if (targetLine == null)
            {
                return null;
            }

            foreach (var chunk in chunks1)
            {
                foreach (var line in chunk.Lines)
                {
                    if (targetLine.OldLineNumber == line.OldLineNumber)
                    {
                        if(targetLine.Content == line.Content)
                        {
                            return line;
                        }
                    }
                }
            }

            return null;
        }

        public static DiffLine MatchLineIgnoreComments(IEnumerable<DiffChunk> chunks1, IList<DiffLine> targetLines)
        {
            var ignoreChars = new[] { ' ', '\t', '/' };

            var targetLine = targetLines.FirstOrDefault();
            if (targetLine == null)
            {
                return null;
            }

            foreach (var chunk in chunks1)
            {
                bool loose = false;
                foreach (var line in chunk.Lines)
                {
                    if (targetLine.OldLineNumber == line.OldLineNumber || loose)
                    {
                        var targetContent = targetLine.Content;
                        var lineContent = line.Content;
                        if (targetContent == lineContent)
                        {
                            return line;
                        }

                        targetContent = GetSignificantContent(targetContent, ignoreChars);
                        lineContent = GetSignificantContent(lineContent, ignoreChars);
                        if (targetContent == lineContent)
                        {
                            if (line.Type == DiffChangeType.Delete)
                            {
                                loose = true;
                            }
                            else
                            {
                                return line;
                            }
                        }
                    }
                }
            }

            return null;
        }

        static string GetSignificantContent(string content, char[] ignoreChars)
        {
            return content.Substring(1).TrimStart(ignoreChars);
        }

        /// Here are some alternative implementations we tried:
        /// https://gist.github.com/shana/200e4719d4f571caab9dbf5921fa5276
        /// Scanning with `text.IndexOf('\n', index)` appears to the the best compromise for average .diff files.
        /// It's likely that `text.IndexOfAny(new [] {'\r', '\n'}, index)` would be faster if lines were much longer.
        public class LineReader
        {
            readonly string text;
            int index = 0;

            public LineReader(string text)
            {
                Guard.ArgumentNotNull(text, nameof(text));

                this.text = text;
            }

            public string ReadLine()
            {
                if (EndOfText)
                {
                    if (StartOfText)
                    {
                        index = -1;
                        return string.Empty;
                    }

                    return null;
                }

                var startIndex = index;
                index = text.IndexOf('\n', index);
                var endIndex = index != -1 ? index : text.Length;
                var length = endIndex - startIndex;

                if (index != -1)
                {
                    if (index > 0 && text[index - 1] == '\r')
                    {
                        length--;
                    }

                    index++;
                }

                return text.Substring(startIndex, length);
            }

            public static int CountCarriageReturns(string text)
            {
                Guard.ArgumentNotNull(text, nameof(text));

                int count = 0;
                int index = 0;
                while ((index = text.IndexOf('\r', index)) != -1)
                {
                    index++;
                    count++;
                }

                return count;
            }

            bool StartOfText => index == 0;

            bool EndOfText => index == -1 || index == text.Length;
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
