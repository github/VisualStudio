using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GitHub.Extensions;
using NUnit.Framework;

namespace UnitTests.GitHub.Extensions
{
    public class GuardTests
    {
        public class TheArgumentNotNullMethod
        {
            [Test]
            public void ShouldNotThrow()
            {
                Guard.ArgumentNotNull(new object(), "name");
            }

            [Test]
            public void ShouldThrow()
            {
                Assert.Throws<ArgumentNullException>(() => Guard.ArgumentNotNull(null, "name"));
            }
        }

        public class TheArgumentNonNegativeMethod
        {
            [Test]
            public void ShouldNotThrowFor0()
            {
                Guard.ArgumentNonNegative(0, "name");
            }

            [Test]
            public void ShouldNotThrowFor1()
            {
                Guard.ArgumentNonNegative(1, "name");
            }

            [Test]
            public void ShouldThrowForMinus1()
            {
                Assert.Throws<ArgumentException>(() => Guard.ArgumentNonNegative(-1, "name"));
            }
        }

        public class TheArgumentNotEmptyStringMethod
        {
            [Test]
            public void ShouldNotThrowForString()
            {
                Guard.ArgumentNotEmptyString("string", "name");
            }

            [Test]
            public void ShouldThrowForEmptyString()
            {
                Assert.Throws<ArgumentException>(() => Guard.ArgumentNotEmptyString("", "name"));
            }

            [Test]
            public void ShouldThrowForNull()
            {
                Assert.Throws<ArgumentException>(() => Guard.ArgumentNotEmptyString(null, "name"));
            }
        }

        public class TheArgumentInRangeMethod
        {
            [Test]
            public void ShouldNotThrowForGreaterThanMinimum()
            {
                Guard.ArgumentInRange(12, 10, "name");
            }

            [Test]
            public void ShouldNotThrowForEqualToMinimumNoMaximum()
            {
                Guard.ArgumentInRange(10, 10, "name");
            }

            [Test]
            public void ShouldNotThrowForEqualToMinimumWithMaximum()
            {
                Guard.ArgumentInRange(10, 10, 20, "name");
            }

            [Test]
            public void ShouldNotThrowForEqualToMaximum()
            {
                Guard.ArgumentInRange(20, 10, 20, "name");
            }

            [Test]
            public void ShouldNotThrowForBetweenMinimumAndMaximum()
            {
                Guard.ArgumentInRange(12, 10, 20, "name");
            }

            [Test]
            public void ShouldThrowForLessThanMinimumNoMaximum()
            {
                Assert.Throws<ArgumentOutOfRangeException>(() => Guard.ArgumentInRange(2, 10, "name"));
            }

            [Test]
            public void ShouldThrowForLessThanMinimumWithMaximum()
            {
                Assert.Throws<ArgumentOutOfRangeException>(() => Guard.ArgumentInRange(2, 10, 20, "name"));
            }

            [Test]
            public void ShouldThrowForGreaterThanMaximum()
            {
                Assert.Throws<ArgumentOutOfRangeException>(() => Guard.ArgumentInRange(22, 10, 20, "name"));
            }
        }
    }
}