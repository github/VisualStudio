using System;
using GitHub.Primitives;
using NUnit.Framework;

namespace UnitTests.GitHub.Exports
{
    public class HostAddressTests
    {
        [Test]
        public void ShouldBeEqualIfAddressesMatch()
        {
            var address1 = HostAddress.Create("foo.com");
            var address2 = HostAddress.Create("foo.com");
            var null1 = default(HostAddress);
            var null2 = default(HostAddress);

            Assert.True(address1.Equals(address2));
            Assert.True(address1 == address2);
            Assert.False(address1 != address2);
            Assert.True(null1 == null2);
        }

        [Test]
        public void ShouldBeNotEqualIfAddressesDontMatch()
        {
            var address1 = HostAddress.Create("foo.com");
            var address2 = HostAddress.Create("bar.com");
            var null1 = default(HostAddress);

            Assert.False(address1.Equals(address2));
            Assert.False(address1 == address2);
            Assert.True(address1 != address2);
            Assert.False(null1 == address1);
            Assert.False(address1 == null1);
            Assert.True(null1 != address1);
            Assert.True(address1 != null1);
        }
    }
}
