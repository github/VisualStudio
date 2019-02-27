using System;
using System.Linq;

namespace GitHub.Models
{
    public class CommitMessage : IEquatable<CommitMessage>
    {
        public string Summary { get; private set; }
        public string Details { get; private set; }
        public string FullMessage { get; private set; }

        /// <summary>
        /// This is for mocking porpoises.
        /// http://cl.ly/image/0q2A2W0U3O2t
        /// </summary>
        public CommitMessage() { }

        public CommitMessage(string fullMessage)
        {
            if (string.IsNullOrEmpty(fullMessage)) return;

            var lines = fullMessage.Replace("\r\n", "\n").Split('\n');
            Summary = lines.FirstOrDefault();

            FullMessage = fullMessage;
            var detailsLines = lines
                .Skip(1)
                .SkipWhile(string.IsNullOrEmpty)
                .ToList();
            Details = detailsLines.Any(x => !string.IsNullOrWhiteSpace(x))
                ? string.Join(Environment.NewLine, detailsLines).Trim()
                : null;
        }

        public bool Equals(CommitMessage other)
        {
            if (ReferenceEquals(other, null))
            {
                return false;
            }

            return string.Equals(Summary, other.Summary, StringComparison.Ordinal)
                && string.Equals(Details, other.Details, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as CommitMessage);
        }

        public override int GetHashCode()
        {
            return Tuple.Create(Summary, Details).GetHashCode();
        }
    }
}
