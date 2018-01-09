/**using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GitHub.Extensions;
using Xunit;

namespace UnitTests.GitHub.Extensions
{
    public class GuardTests
    {
        public class TheArgumentNotNullMethod : TestBaseClass
        {
            [Fact]
            public void ShouldNotThrow()
            {
                Guard.ArgumentNotNull(new object(), "name");
            }

            [Fact]
            public void ShouldThrow()
            {
                Assert.Throws<ArgumentNullException>(() => Guard.ArgumentNotNull(null, "name"));
            }
        }

        public class TheArgumentNonNegativeMethod : TestBaseClass
        {
            [Fact]
            public void ShouldNotThrowFor0()
            {
                Guard.ArgumentNonNegative(0, "name");
            }

            [Fact]
            public void ShouldNotThrowFor1()
            {
                Guard.ArgumentNonNegative(1, "name");
            }

            [Fact]
            public void ShouldThrowForMinus1()
            {
                Assert.Throws<ArgumentException>(() => Guard.ArgumentNonNegative(-1, "name"));
            }
        }

        public class TheArgumentNotEmptyStringMethod : TestBaseClass
        {
            [Fact]
            public void ShouldNotThrowForString()
            {
                Guard.ArgumentNotEmptyString("string", "name");
            }

            [Fact]
            public void ShouldThrowForEmptyString()
            {
                Assert.Throws<ArgumentException>(() => Guard.ArgumentNotEmptyString("", "name"));
            }

            [Fact]
            public void ShouldThrowForNull()
            {
                Assert.Throws<ArgumentException>(() => Guard.ArgumentNotEmptyString(null, "name"));
            }
        }

        public class TheArgumentInRangeMethod : TestBaseClass
        {
            [Fact]
            public void ShouldNotThrowForGreaterThanMinimum()
            {
                Guard.ArgumentInRange(12, 10, "name");
            }

            [Fact]
            public void ShouldNotThrowForEqualToMinimumNoMaximum()
            {
                Guard.ArgumentInRange(10, 10, "name");
            }

            [Fact]
            public void ShouldNotThrowForEqualToMinimumWithMaximum()
            {
                Guard.ArgumentInRange(10, 10, 20, "name");
            }

            [Fact]
            public void ShouldNotThrowForEqualToMaximum()
            {
                Guard.ArgumentInRange(20, 10, 20, "name");
            }

            [Fact]
            public void ShouldNotThrowForBetweenMinimumAndMaximum()
            {
                Guard.ArgumentInRange(12, 10, 20, "name");
            }

            [Fact]
            public void ShouldThrowForLessThanMinimumNoMaximum()
            {
                Assert.Throws<ArgumentOutOfRangeException>(() => Guard.ArgumentInRange(2, 10, "name"));
            }

            [Fact]
            public void ShouldThrowForLessThanMinimumWithMaximum()
            {
                Assert.Throws<ArgumentOutOfRangeException>(() => Guard.ArgumentInRange(2, 10, 20, "name"));
            }

            [Fact]
            public void ShouldThrowForGreaterThanMaximum()
            {
                Assert.Throws<ArgumentOutOfRangeException>(() => Guard.ArgumentInRange(22, 10, 20, "name"));
            }
        }
    }
}
*/