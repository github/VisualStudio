using System;
using System.IO;
using System.Linq;
using GitHub.Models;
using NUnit.Framework;

namespace GitHub.InlineReviews.UnitTests.Models
{
    public class DiffUtilitiesTests
    {
        public class TheParseFragmentMethod
        {
            [Test]
            public void EmptyDiff_NoDiffChunks()
            {
                var chunks = DiffUtilities.ParseFragment("");

				Assert.That(chunks, Is.Empty);
            }

            [TestCase("@@ -1 +1 @@")]
			[TestCase("@@ -1 +1,0 @@")]
			[TestCase("@@ -1,0 +1 @@")]
			[TestCase("@@ -1,0 +1,0 @@")]
			[TestCase("@@ -1,0 +1,0 @@ THIS IS A COMMENT THAT WILL BE IGNORED")]
			public void HeaderOnly_OneChunkNoLines(string header)
            {
                var chunks = DiffUtilities.ParseFragment(header);

                Assert.That(chunks, Has.One.Items);
				var chunk = chunks.First();
				Assert.That(chunk.Lines, Is.Empty);
            }

			[TestCase("@@ -1 +2 @@", 1, 2)]
			[TestCase("@@ -1 +2,0 @@", 1, 2)]
			[TestCase("@@ -1,0 +2 @@", 1, 2)]
			[TestCase("@@ -1,0 +2,0 @@", 1, 2)]
			[TestCase("@@ -1,0 +2,0 @@ THIS IS A COMMENT THAT WILL BE IGNORED", 1, 2)]
            [TestCase(
@"diff --git a/src/Foo.cs b/src/Foo.cs
index b02decb..f7dadae 100644
--- a/src/Foo.cs
+++ b/src/Foo.cs
@@ -1 +2 @@", 1, 2)]

			public void HeaderOnly_OldAndNewLineNumbers(string header, int expectOldLineNumber, int expectNewLineNumber)
            {
                var chunks = DiffUtilities.ParseFragment(header);
                var chunk = chunks.First();

                Assert.That(expectOldLineNumber, Is.EqualTo(chunk.OldLineNumber));
                Assert.That(expectNewLineNumber, Is.EqualTo(chunk.NewLineNumber));
            }

            [Test]
            public void HeaderOnlyNoNewLineAtEnd_NoLines()
            {
                var header =
@"@@ -1 +1 @@
\ No newline at end of file\n";

                var chunks = DiffUtilities.ParseFragment(header);

                var chunk = chunks.First();
				Assert.That(chunk.Lines, Is.Empty);
            }

            [Test]
            public void NoNewLineNotAtEndOfChunk_CheckLineCount()
            {
                var header =
@"@@ -1 +1 @@
-old
\ No newline at end of file
+new";

                var chunk = DiffUtilities.ParseFragment(header).First();

                Assert.That(2, Is.EqualTo(chunk.Lines.Count()));
            }

            [Test]
            public void NoNewLineNotAtEndOfChunk_CheckDiffLineNumber()
            {
                var header =
@"@@ -1 +1 @@
-old
\ No newline at end of file
+new";

                var chunk = DiffUtilities.ParseFragment(header).First();

                var line = chunk.Lines.Last();
                Assert.That(3, Is.EqualTo(line.DiffLineNumber));
            }

			[TestCase("+foo\n+bar\n", "+foo", "+bar")]
			[TestCase("+fo\ro\n+bar\n", "+fo\ro", "+bar")]
			[TestCase("+foo\r\r\n+bar\n", "+foo\r", "+bar")]
			[TestCase("+\\r\n+\r\n", "+\\r", "+")]
			public void FirstChunk_CheckLineContent(string diffLines, string contentLine0, string contentLine1)
            {
                var header = "@@ -1 +1 @@";
                var diff = header + "\n" + diffLines;

                var chunk = DiffUtilities.ParseFragment(diff).First();

                Assert.That(contentLine0, Is.EqualTo(chunk.Lines[0].Content));
                Assert.That(contentLine1, Is.EqualTo(chunk.Lines[1].Content));
            }

			[TestCase("+foo\n+bar\n", 1, 2)]
			[TestCase("+fo\ro\n+bar\n", 1, 3)]
			[TestCase("+foo\r\r\n+bar\n", 1, 3)]
			public void FirstChunk_CheckNewLineNumber(string diffLines, int lineNumber0, int lineNumber1)
            {
                var header = "@@ -1 +1 @@";
                var diff = header + "\n" + diffLines;

                var chunk = DiffUtilities.ParseFragment(diff).First();

                Assert.That(lineNumber0, Is.EqualTo(chunk.Lines[0].NewLineNumber));
                Assert.That(lineNumber1, Is.EqualTo(chunk.Lines[1].NewLineNumber));
            }

			[TestCase("-foo\n-bar\n", 1, 2)]
			[TestCase("-fo\ro\n-bar\n", 1, 3)]
			[TestCase("-foo\r\r\n-bar\n", 1, 3)]
			public void FirstChunk_CheckOldLineNumber(string diffLines, int lineNumber0, int lineNumber1)
            {
                var header = "@@ -1 +1 @@";
                var diff = header + "\n" + diffLines;

                var chunk = DiffUtilities.ParseFragment(diff).First();

                Assert.That(lineNumber0, Is.EqualTo(chunk.Lines[0].OldLineNumber));
                Assert.That(lineNumber1, Is.EqualTo(chunk.Lines[1].OldLineNumber));
            }

            [Test]
            public void FirstChunk_CheckDiffLineZeroBased()
            {
                var expectDiffLine = 0;
                var header = "@@ -1 +1 @@";

                var chunk = DiffUtilities.ParseFragment(header).First();

                Assert.That(expectDiffLine, Is.EqualTo(chunk.DiffLine));
            }

			[TestCase(1, 2)]
			public void FirstChunk_CheckLineNumbers(int oldLineNumber, int newLineNumber)
            {
                var header = $"@@ -{oldLineNumber} +{newLineNumber} @@";

                var chunk = DiffUtilities.ParseFragment(header).First();

                Assert.That(oldLineNumber, Is.EqualTo(chunk.OldLineNumber));
                Assert.That(newLineNumber, Is.EqualTo(chunk.NewLineNumber));
            }

			[TestCase(1, 2, " 1", 1, 2)]
			[TestCase(1, 2, "+1", -1, 2)]
			[TestCase(1, 2, "-1", 1, -1)]
			public void FirstLine_CheckLineNumbers(int oldLineNumber, int newLineNumber, string line, int expectOldLineNumber, int expectNewLineNumber)
            {
                var header = $"@@ -{oldLineNumber} +{newLineNumber} @@\n{line}";

                var chunk = DiffUtilities.ParseFragment(header).First();
                var diffLine = chunk.Lines.First();

                Assert.That(expectOldLineNumber, Is.EqualTo(diffLine.OldLineNumber));
                Assert.That(expectNewLineNumber, Is.EqualTo(diffLine.NewLineNumber));
            }

			[TestCase(" 1", 0, 1)]
			[TestCase(" 1\n 2", 1, 2)]
			[TestCase(" 1\n 2\n 3", 2, 3)]
			public void SkipNLines_CheckDiffLineNumber(string lines, int skip, int expectDiffLineNumber)
            {
                var fragment = $"@@ -1 +1 @@\n{lines}";

                var result = DiffUtilities.ParseFragment(fragment);

                var firstLine = result.First().Lines.Skip(skip).First();
                Assert.That(expectDiffLineNumber, Is.EqualTo(firstLine.DiffLineNumber));
            }

			[TestCase(" FIRST")]
			[TestCase("+FIRST")]
			[TestCase("-FIRST")]
			public void FirstLine_CheckToString(string line)
            {
                var fragment = $"@@ -1 +1 @@\n{line}";
                var result = DiffUtilities.ParseFragment(fragment);
                var firstLine = result.First().Lines.First();

                var str = firstLine.ToString();

                Assert.That(line, Is.EqualTo(str));
            }

			[TestCase(" FIRST")]
			[TestCase("+FIRST")]
			[TestCase("-FIRST")]
			public void FirstLine_CheckContent(string line)
            {
                var fragment = $"@@ -1,4 +1,4 @@\n{line}";

                var result = DiffUtilities.ParseFragment(fragment);
                var firstLine = result.First().Lines.First();

                Assert.That(line, Is.EqualTo(firstLine.Content));
            }

			[TestCase(" FIRST", DiffChangeType.None)]
			[TestCase("+FIRST", DiffChangeType.Add)]
			[TestCase("-FIRST", DiffChangeType.Delete)]
			public void FirstLine_CheckDiffChangeTypes(string line, DiffChangeType expectType)
            {
                var fragment = $"@@ -1 +1 @@\n{line}";

                var result = DiffUtilities.ParseFragment(fragment);

                var firstLine = result.First().Lines.First();
                Assert.That(expectType, Is.EqualTo(firstLine.Type));
            }

			[TestCase("?FIRST", "Invalid diff line change char: '?'.")]
			public void InvalidDiffLineChangeChar(string line, string expectMessage)
            {
                var fragment = $"@@ -1,4 +1,4 @@\n{line}";

                var result = DiffUtilities.ParseFragment(fragment);
                var e = Assert.Throws<InvalidDataException>(() => result.First());

                Assert.That(expectMessage, Is.EqualTo(e.Message));
            }
        }

        public class TheMatchMethod
        {
            /// <param name="diffLines">Target diff chunk with header (with '.' as line separator)</param>
            /// <param name="matchLines">Diff lines to match (with '.' as line separator)</param>
            /// <param name="expectedDiffLineNumber">The DiffLineNumber that the last line of matchLines falls on</param>
            [TestCase(" 1", " 1", 1)]
            [TestCase(" 1. 2", " 2", 2)]
            [TestCase(" 1. 1", " 1", 2)] // match the later line
            [TestCase("+x", "-x", -1)]
            [TestCase("", " x", -1)]
            [TestCase(" x", "", -1)]

            [TestCase(" 1. 2.", " 1. 2.", 2)] // matched full context
            [TestCase(" 1. 2.", " 3. 2.", -1)] // didn't match full context
            [TestCase(" 2.", " 1. 2.", 1)] // match if we run out of context lines

            // Tests for https://github.com/github/VisualStudio/issues/1149
            // Matching algorithm got confused when there was a partial match.
            [TestCase("+a.+x.+x.", "+a.+x.", 2)]
            [TestCase("+a.+x.+x.", "+a.+x.+x.", 3)]
            [TestCase("+a.+x.+x.+b.+x.+x.", "+a.+x.", 2)]
            [TestCase("+a.+x.+x.+b.+x.+x.", "+b.+x.", 5)]
            [TestCase("+a.+b.+x", "+a.+x.", -1)] // backtrack when there is a failed match
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
                Assert.That(expectedDiffLineNumber, Is.EqualTo(diffLineNumber));
            }

            [Test]
            public void MatchSameLine()
            {
                var diff = "@@ -1 +1 @@\n 1";
                var chunks1 = DiffUtilities.ParseFragment(diff).ToList();
                var chunks2 = DiffUtilities.ParseFragment(diff).ToList();
                var expectLine = chunks1.First().Lines.First();
                var targetLine = chunks2.First().Lines.First();
                var targetLines = new[] { targetLine };

                var line = DiffUtilities.Match(chunks1, targetLines);

                Assert.That(expectLine, Is.EqualTo(line));
            }

            [Test]
            public void NoLineMatchesFromNoLines()
            {
                var chunks = Array.Empty<DiffChunk>();
                var lines = Array.Empty<DiffLine>();

                var line = DiffUtilities.Match(chunks, lines);

                Assert.That(line, Is.Null);
            }
        }

        public class TheLineReaderClass
        {
            [TestCase("", new[] { "", null })]
            [TestCase("\n", new[] { "", null })]
            [TestCase("\r\n", new[] { "", null })]
            [TestCase("1", new[] { "1", null })]
            [TestCase("1\n2\n", new[] { "1", "2", null })]
            [TestCase("1\n2", new[] { "1", "2", null })]
            [TestCase("1\r\n2\n", new[] { "1", "2", null })]
            [TestCase("1\r\n2", new[] { "1", "2", null })]
            [TestCase("\r", new[] { "\r", null })]
            [TestCase("\r\r", new[] { "\r\r", null })]
            [TestCase("\r\r\n", new[] { "\r", null })]
            [TestCase("\r_\n", new[] { "\r_", null })]
            public void ReadLines(string text, string[] expectLines)
            {
                var lineReader = new DiffUtilities.LineReader(text);
            
                foreach (var expectLine in expectLines)
                {
                    var line = lineReader.ReadLine();
                    Assert.That(expectLine, Is.EqualTo(line));
                }
            }

            [Test]
            public void Constructor_NullText_ArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(() => new DiffUtilities.LineReader(null));
            }

            [TestCase("", 0)]
            [TestCase("\r", 1)]
            [TestCase("\r\n", 1)]
            [TestCase("\r\r", 2)]
            [TestCase("\r-\r", 2)]
            public void CountCarriageReturns(string text, int expectCount)
            {
                var count = DiffUtilities.LineReader.CountCarriageReturns(text);

                Assert.That(expectCount, Is.EqualTo(count));
            }

            [Test]
            public void CountCarriageReturns_NullText_ArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(() => DiffUtilities.LineReader.CountCarriageReturns(null));
            }
        }
    }
}
