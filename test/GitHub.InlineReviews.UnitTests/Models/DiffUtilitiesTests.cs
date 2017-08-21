using System;
using System.IO;
using System.Linq;
using GitHub.Models;
using Xunit;

namespace GitHub.InlineReviews.UnitTests.Models
{
    public class DiffUtilitiesTests
    {
        public class TheParseFragmentMethod
        {
            [Fact]
            public void EmptyDiff_NoDiffChunks()
            {
                var chunks = DiffUtilities.ParseFragment("");

                Assert.Equal(0, chunks.Count());
            }

            [Theory]
            [InlineData("@@ -1 +1 @@")]
            [InlineData("@@ -1 +1,0 @@")]
            [InlineData("@@ -1,0 +1 @@")]
            [InlineData("@@ -1,0 +1,0 @@")]
            [InlineData("@@ -1,0 +1,0 @@ THIS IS A COMMENT THAT WILL BE IGNORED")]
            public void HeaderOnly_OneChunkNoLines(string header)
            {
                var chunks = DiffUtilities.ParseFragment(header);

                Assert.Equal(1, chunks.Count());
                var chunk = chunks.First();
                Assert.Equal(0, chunk.Lines.Count());
            }

            [Fact]
            public void HeaderOnlyNoNewLineAtEnd_NoLines()
            {
                var header =
@"@@ -1 +1 @@
\ No newline at end of file\n";

                var chunks = DiffUtilities.ParseFragment(header);

                var chunk = chunks.First();
                Assert.Equal(0, chunk.Lines.Count());
            }

            [Fact]
            public void NoNewLineNotAtEndOfChunk_CheckLineCount()
            {
                var header =
@"@@ -1 +1 @@
-old
\ No newline at end of file
+new";

                var chunk = DiffUtilities.ParseFragment(header).First();

                Assert.Equal(2, chunk.Lines.Count());
            }

            [Fact]
            public void NoNewLineNotAtEndOfChunk_CheckDiffLineNumber()
            {
                var header =
@"@@ -1 +1 @@
-old
\ No newline at end of file
+new";

                var chunk = DiffUtilities.ParseFragment(header).First();

                var line = chunk.Lines.Last();
                Assert.Equal(3, line.DiffLineNumber);
            }

            [Fact]
            public void FirstChunk_CheckDiffLineZeroBased()
            {
                var expectDiffLine = 0;
                var header = "@@ -1 +1 @@";

                var chunk = DiffUtilities.ParseFragment(header).First();

                Assert.Equal(expectDiffLine, chunk.DiffLine);
            }

            [Theory]
            [InlineData(1, 2)]
            public void FirstChunk_CheckLineNumbers(int oldLineNumber, int newLineNumber)
            {
                var header = $"@@ -{oldLineNumber} +{newLineNumber} @@";

                var chunk = DiffUtilities.ParseFragment(header).First();

                Assert.Equal(oldLineNumber, chunk.OldLineNumber);
                Assert.Equal(newLineNumber, chunk.NewLineNumber);
            }

            [Theory]
            [InlineData(1, 2, " 1", 1, 2)]
            [InlineData(1, 2, "+1", -1, 2)]
            [InlineData(1, 2, "-1", 1, -1)]
            public void FirstLine_CheckLineNumbers(int oldLineNumber, int newLineNumber, string line, int expectOldLineNumber, int expectNewLineNumber)
            {
                var header = $"@@ -{oldLineNumber} +{newLineNumber} @@\n{line}";

                var chunk = DiffUtilities.ParseFragment(header).First();
                var diffLine = chunk.Lines.First();

                Assert.Equal(expectOldLineNumber, diffLine.OldLineNumber);
                Assert.Equal(expectNewLineNumber, diffLine.NewLineNumber);
            }

            [Theory]
            [InlineData(" 1", 0, 1)]
            [InlineData(" 1\n 2", 1, 2)]
            [InlineData(" 1\n 2\n 3", 2, 3)]
            public void SkipNLines_CheckDiffLineNumber(string lines, int skip, int expectDiffLineNumber)
            {
                var fragment = $"@@ -1 +1 @@\n{lines}";

                var result = DiffUtilities.ParseFragment(fragment);

                var firstLine = result.First().Lines.Skip(skip).First();
                Assert.Equal(expectDiffLineNumber, firstLine.DiffLineNumber);
            }

            [Theory]
            [InlineData(" FIRST")]
            [InlineData("+FIRST")]
            [InlineData("-FIRST")]
            public void FirstLine_CheckToString(string line)
            {
                var fragment = $"@@ -1 +1 @@\n{line}";
                var result = DiffUtilities.ParseFragment(fragment);
                var firstLine = result.First().Lines.First();

                var str = firstLine.ToString();

                Assert.Equal(line, str);
            }

            [Theory]
            [InlineData(" FIRST")]
            [InlineData("+FIRST")]
            [InlineData("-FIRST")]
            public void FirstLine_CheckContent(string line)
            {
                var fragment = $"@@ -1,4 +1,4 @@\n{line}";

                var result = DiffUtilities.ParseFragment(fragment);
                var firstLine = result.First().Lines.First();

                Assert.Equal(line, firstLine.Content);
            }

            [Theory]
            [InlineData(" FIRST", DiffChangeType.None)]
            [InlineData("+FIRST", DiffChangeType.Add)]
            [InlineData("-FIRST", DiffChangeType.Delete)]
            public void FirstLine_CheckDiffChangeTypes(string line, DiffChangeType expectType)
            {
                var fragment = $"@@ -1 +1 @@\n{line}";

                var result = DiffUtilities.ParseFragment(fragment);

                var firstLine = result.First().Lines.First();
                Assert.Equal(expectType, firstLine.Type);
            }

            [Theory]
            [InlineData("?FIRST", "Invalid diff line change char: '?'.")]
            public void InvalidDiffLineChangeChar(string line, string expectMessage)
            {
                var fragment = $"@@ -1,4 +1,4 @@\n{line}";

                var result = DiffUtilities.ParseFragment(fragment);
                var e = Assert.Throws<InvalidDataException>(() => result.First());

                Assert.Equal(expectMessage, e.Message);
            }
        }

        public class TheMatchMethod
        {
            [Theory]
            [InlineData(" 1", " 1", 0)]
            [InlineData(" 1. 2", " 2", 1)]
            [InlineData(" 1. 1", " 1", 1)] // match the later line
            [InlineData("+x", "-x", -1)]
            [InlineData("", " x", -1)]
            [InlineData(" x", "", -1)]

            [InlineData(" 1. 2.", " 1. 2.", 1)] // matched full context
            [InlineData(" 1. 2.", " 3. 2.", -1)] // didn't match full context
            [InlineData(" 2.", " 1. 2.", 0)] // match if we run out of context lines

            // Tests for https://github.com/github/VisualStudio/issues/1149
            // Matching algorithm got confused when there was a partial match.
            [InlineData("+a.+x.+x.", "+a.+x.", 1)]
            [InlineData("+a.+x.+x.", "+a.+x.+x.", 2)]
            [InlineData("+a.+x.+x.+b.+x.+x.", "+a.+x.", 1)]
            [InlineData("+a.+x.+x.+b.+x.+x.", "+b.+x.", 4)]
            [InlineData("+a.+b.+x", "+a.+x.", -1)] // backtrack when there is a failed match
            public void MatchLine(string lines1, string lines2, int skip /* -1 for no match */)
            {
                var header = "@@ -1 +1 @@";
                lines1 = lines1.Replace(".", "\r\n");
                lines2 = lines2.Replace(".", "\r\n");
                var chunks1 = DiffUtilities.ParseFragment(header + "\n" + lines1).ToList();
                var chunks2 = DiffUtilities.ParseFragment(header + "\n" + lines2).ToList();
                var expectLine = (skip != -1) ? chunks1.First().Lines[skip] : null;
                var targetLines = chunks2.First().Lines.Reverse().ToList();

                var line = DiffUtilities.Match(chunks1, targetLines);

                Assert.Equal(expectLine, line);
            }

            [Fact]
            public void MatchSameLine()
            {
                var diff = "@@ -1 +1 @@\n 1";
                var chunks1 = DiffUtilities.ParseFragment(diff).ToList();
                var chunks2 = DiffUtilities.ParseFragment(diff).ToList();
                var expectLine = chunks1.First().Lines.First();
                var targetLine = chunks2.First().Lines.First();
                var targetLines = new[] { targetLine };

                var line = DiffUtilities.Match(chunks1, targetLines);

                Assert.Equal(expectLine, line);
            }

            [Fact]
            public void NoLineMatchesFromNoLines()
            {
                var chunks = new DiffChunk[0];
                var lines = new DiffLine[0];

                var line = DiffUtilities.Match(chunks, lines);

                Assert.Equal(null, line);
            }
        }
    }
}
