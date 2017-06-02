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

            [Fact]
            public void HeaderOnly_NoLines()
            {
                var header = "@@ -1,0 +1,0 @@";

                var chunks = DiffUtilities.ParseFragment(header);

                var chunk = chunks.First();
                Assert.Equal(0, chunk.Lines.Count());
            }

            [Fact]
            public void Chunks_ZeroBased()
            {
                var header = "@@ -1,1 +1,1 @@\n 1";

                var chunk = DiffUtilities.ParseFragment(header).First();

                Assert.Equal(0, chunk.DiffLine);
            }

            [Fact]
            public void FirstDiffLine_HasDiffLineNumber1()
            {
                var expectLine = " FIRST";
                var expectDiffLineNumber = 1;
                var fragment = $"@@ -1,4 +1,4 @@\n{expectLine}";

                var result = DiffUtilities.ParseFragment(fragment);

                var firstLine = result.First().Lines.First();
                Assert.Equal(expectLine, firstLine.Content);
                Assert.Equal(expectDiffLineNumber, firstLine.DiffLineNumber);
            }

            [Fact]
            public void TextOnSameLineAsHeader_IgnoreLine()
            {
                var expectLine = " FIRST";
                var expectDiffLineNumber = 1;
                var fragment = $"@@ -10,7 +10,6 @@ TextOnSameLineAsHeader\n{expectLine}";

                var result = DiffUtilities.ParseFragment(fragment);

                var firstLine = result.First().Lines.First();
                Assert.Equal(expectLine, firstLine.Content);
                Assert.Equal(expectDiffLineNumber, firstLine.DiffLineNumber);
            }

            [Theory]
            [InlineData(" FIRST", DiffChangeType.None)]
            [InlineData("+FIRST", DiffChangeType.Add)]
            [InlineData("-FIRST", DiffChangeType.Delete)]
            public void DiffChangeTypes(string line, DiffChangeType expectType)
            {
                var fragment = $"@@ -1,4 +1,4 @@\n{line}";

                var result = DiffUtilities.ParseFragment(fragment);

                var firstLine = result.First().Lines.First();
                Assert.Equal(line, firstLine.Content);
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
            [Fact]
            public void MatchSameLine()
            {
                var diff = "@@ -1,1 +1,0 @@\n-1";
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
