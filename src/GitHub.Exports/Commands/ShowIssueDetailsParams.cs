using System;
using GitHub.Primitives;

namespace GitHub.Commands
{
    public class ShowIssueDetailsParams
    {
        public ShowIssueDetailsParams(
            HostAddress address,
            string owner,
            string repository,
            int number)
        {
            Address = address;
            Owner = owner;
            Repository = repository;
            Number = number;
        }

        public HostAddress Address { get; }
        public string Owner { get; }
        public string Repository { get; }
        public int Number { get; }
    }
}
