using System;
using GitHub.Models;
using NUnit.Framework;

namespace GitHub.Exports.UnitTests
{
    public class ScopesCollectionTests
    {
        [Test]
        public void Matches_Returns_False_When_Missing_Scopes()
        {
            var required = new[] { "user", "repo", "gist", "write:public_key" };
            var target = new ScopesCollection(new[] { "user", "repo", "write:public_key" });

            Assert.False(target.Matches(required));
        }

        [Test]
        public void Returns_True_When_Scopes_Equal()
        {
            var required = new[] { "user", "repo", "gist", "write:public_key" };
            var target = new ScopesCollection(new[] { "user", "repo", "gist", "write:public_key" });

            Assert.True(target.Matches(required));
        }

        [Test]
        public void Returns_True_When_Extra_Scopes_Returned()
        {
            var required = new[] { "user", "repo", "gist", "write:public_key" };
            var target = new ScopesCollection(new[] { "user", "repo", "gist", "foo", "write:public_key" });

            Assert.True(target.Matches(required));
        }

        [Test]
        public void Returns_True_When_Admin_Scope_Returned_Instead_Of_Write()
        {
            var required = new[] { "user", "repo", "gist", "write:public_key" };
            var target = new ScopesCollection(new[] { "user", "repo", "gist", "foo", "admin:public_key" });

            Assert.True(target.Matches(required));
        }
    }
}
