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

            [Theory]
            [InlineData("@@ -1 +2 @@", 1, 2)]
            [InlineData("@@ -1 +2,0 @@", 1, 2)]
            [InlineData("@@ -1,0 +2 @@", 1, 2)]
            [InlineData("@@ -1,0 +2,0 @@", 1, 2)]
            [InlineData("@@ -1,0 +2,0 @@ THIS IS A COMMENT THAT WILL BE IGNORED", 1, 2)]
            [InlineData(
@"diff --git a/src/Foo.cs b/src/Foo.cs
index b02decb..f7dadae 100644
--- a/src/Foo.cs
+++ b/src/Foo.cs
@@ -1 +2 @@", 1, 2)] // Extra header info when using `Diff.Compare<Patch>`.
            public void HeaderOnly_OldAndNewLineNumbers(string header, int expectOldLineNumber, int expectNewLineNumber)
            {
                var chunks = DiffUtilities.ParseFragment(header);
                var chunk = chunks.First();

                Assert.Equal(expectOldLineNumber, chunk.OldLineNumber);
                Assert.Equal(expectNewLineNumber, chunk.NewLineNumber);
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

            [Theory]
            [InlineData("+foo\n+bar\n", "+foo", "+bar")]
            [InlineData("+fo\ro\n+bar\n", "+fo\ro", "+bar")]
            [InlineData("+foo\r\r\n+bar\n", "+foo\r", "+bar")]
            [InlineData("+\\r\n+\r\n", "+\\r", "+")]
            public void FirstChunk_CheckLineContent(string diffLines, string contentLine0, string contentLine1)
            {
                var header = "@@ -1 +1 @@";
                var diff = header + "\n" + diffLines;

                var chunk = DiffUtilities.ParseFragment(diff).First();

                Assert.Equal(contentLine0, chunk.Lines[0].Content);
                Assert.Equal(contentLine1, chunk.Lines[1].Content);
            }

            [Theory]
            [InlineData("+foo\n+bar\n", 1, 2)]
            [InlineData("+fo\ro\n+bar\n", 1, 3)]
            [InlineData("+foo\r\r\n+bar\n", 1, 3)]
            public void FirstChunk_CheckNewLineNumber(string diffLines, int lineNumber0, int lineNumber1)
            {
                var header = "@@ -1 +1 @@";
                var diff = header + "\n" + diffLines;

                var chunk = DiffUtilities.ParseFragment(diff).First();

                Assert.Equal(lineNumber0, chunk.Lines[0].NewLineNumber);
                Assert.Equal(lineNumber1, chunk.Lines[1].NewLineNumber);
            }

            [Theory]
            [InlineData("-foo\n-bar\n", 1, 2)]
            [InlineData("-fo\ro\n-bar\n", 1, 3)]
            [InlineData("-foo\r\r\n-bar\n", 1, 3)]
            public void FirstChunk_CheckOldLineNumber(string diffLines, int lineNumber0, int lineNumber1)
            {
                var header = "@@ -1 +1 @@";
                var diff = header + "\n" + diffLines;

                var chunk = DiffUtilities.ParseFragment(diff).First();

                Assert.Equal(lineNumber0, chunk.Lines[0].OldLineNumber);
                Assert.Equal(lineNumber1, chunk.Lines[1].OldLineNumber);
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
            /// <param name="diffLines">Target diff chunk with header (with '.' as line separator)</param>
            /// <param name="matchLines">Diff lines to match (with '.' as line separator)</param>
            /// <param name="expectedDiffLineNumber">The DiffLineNumber that the last line of matchLines falls on</param>
            [Theory]
            [InlineData(" 1", " 1", 1)]
            [InlineData(" 1. 2", " 2", 2)]
            [InlineData(" 1. 1", " 1", 2)] // match the later line
            [InlineData("+x", "-x", -1)]
            [InlineData("", " x", -1)]
            [InlineData(" x", "", -1)]

            [InlineData(" 1. 2.", " 1. 2.", 2)] // matched full context
            [InlineData(" 1. 2.", " 3. 2.", -1)] // didn't match full context
            [InlineData(" 2.", " 1. 2.", 1)] // match if we run out of context lines

            // Tests for https://github.com/github/VisualStudio/issues/1149
            // Matching algorithm got confused when there was a partial match.
            [InlineData("+a.+x.+x.", "+a.+x.", 2)]
            [InlineData("+a.+x.+x.", "+a.+x.+x.", 3)]
            [InlineData("+a.+x.+x.+b.+x.+x.", "+a.+x.", 2)]
            [InlineData("+a.+x.+x.+b.+x.+x.", "+b.+x.", 5)]
            [InlineData("+a.+b.+x", "+a.+x.", -1)] // backtrack when there is a failed match
            public void MatchLine(string diffLines, string matchLines, int expectedDiffLineNumber /* -1 for no match */)
            {
                var header = "@@ -1 +1 @@";
                diffLines = diffLines.Replace(".", "\r\n");
                matchLines = matchLines.Replace(".", "\r\n");
                var chunks1 = DiffUtilities.ParseFragment(header + "\n" + diffLines).ToList();
                var chunks2 = DiffUtilities.ParseFragment(header + "\n" + matchLines).ToList();
                var targetLines = chunks2.First().Lines.Reverse().ToList();

                var line = DiffUtilities.Match(chunks1, targetLines);

                var diffLineNumber = (line != null) ? line.DiffLineNumber : -1;
                Assert.Equal(expectedDiffLineNumber, diffLineNumber);
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

        public class TheLineReaderClass
        {
            [Theory]
            [InlineData(null, new string[] { "", null })]
            [InlineData("", new string[] { "", null })]
            [InlineData("\n", new string[] { "", null })]
            [InlineData("\r\n", new string[] { "", null })]
            [InlineData("1", new string[] { "1", null })]
            [InlineData("1\n2\n", new string[] { "1", "2", null })]
            [InlineData("1\n2", new string[] { "1", "2", null })]
            [InlineData("1\r\n2\n", new string[] { "1", "2", null })]
            [InlineData("1\r\n2", new string[] { "1", "2", null })]
            [InlineData("\r", new string[] { "\r", null, null })]
            [InlineData("\r\r", new string[] { "\r\r", null })]
            [InlineData("\r\r\n", new string[] { "\r", null })]
            [InlineData("\r_\n", new string[] { "\r_", null })]
            public void ReadLines(string text, string[] expectedLines)
            {
                if (text == null)
                {
                    Assert.Throws<ArgumentNullException>(() => new DiffUtilities.LineReader(text));
                    return;
                }
                var lineReader = new DiffUtilities.LineReader(text);

                foreach (var expectedLine in expectedLines)
                {
                    var lineAndCarriageReturns = lineReader.ReadLine();
                    Assert.Equal(expectedLine, lineAndCarriageReturns.Line);
                    Assert.Equal(expectedLine?.Count(c => c == '\r') ?? 0, lineAndCarriageReturns.CarriageReturns);
                }
            }
        }
    }
}
