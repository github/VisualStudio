using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using GitHub.Extensions;

namespace GitHub.Models
{
    public static class DiffUtilities
    {
        static readonly Regex ChunkHeaderRegex = new Regex(@"^@@\s+\-(\d+),?\d*\s+\+(\d+),?\d*\s@@");

        public static IEnumerable<DiffChunk> ParseFragment(string diff)
        {
            var reader = new LineReader(diff);
            LineReader.LineInformation lineInfo;
            DiffChunk chunk = null;
            int diffLine = -1;
            int oldLine = -1;
            int newLine = -1;

            while ((lineInfo = reader.ReadLine()).Line != null)
            {
                var line = lineInfo.Line;
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

                        var lineCount = lineInfo.CarriageReturns + 1;

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

        public class LineReader
        {
            static readonly LineInformation Default = new LineInformation(null, 0);
            static readonly LineInformation Empty = new LineInformation(String.Empty, 0);
            readonly string text;
            readonly int length = 0;
            int index = 0;

            public LineReader(string text)
            {
                Guard.ArgumentNotNull(text, nameof(text));
                this.text = text;
                this.length = text.Length;
            }

            public LineInformation ReadLine()
            {
                if (EndOfText)
                {
                    if (StartOfText)
                    {
                        index = -1;
                        return Empty;
                    }
                    return Default;
                }

                var carriageReturns = 0;
                StringBuilder sb = new StringBuilder();
                for (; index < length; index++)
                {
                    if (text[index] == '\n')
                    {
                        index++;
                        break;
                    }
                    else if (text[index] == '\r')
                    {
                        // if we're at the end or the next character isn't a new line, count this carriage return
                        if (index == length - 1 || index < length - 1 && text[index + 1] != '\n')
                        {
                            carriageReturns++;
                            sb.Append(text[index]);
                        }
                    }
                    else
                    {
                        sb.Append(text[index]);
                    }
                }

                return new LineInformation(sb.ToString(), carriageReturns);
            }

            bool StartOfText => index == 0;

            bool EndOfText => index == -1 || index == length;

            public class LineInformation
            {
                public string Line { get; private set; }
                public int CarriageReturns { get; private set; }
                public LineInformation(string line, int carriageReturns)
                {
                    this.Line = line;
                    this.CarriageReturns = carriageReturns;
                }
            }
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
