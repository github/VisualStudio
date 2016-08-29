using NullGuard;
using System;
using System.Collections.Generic;

namespace GitHub.Collections
{
    public class LambdaComparer<T> : IEqualityComparer<T>, IComparer<T>
    {
        readonly Func<T, T, int> lambdaComparer;
        readonly Func<T, int> lambdaHash;

        public LambdaComparer(Func<T, T, int> lambdaComparer) :
            this(lambdaComparer, null)
        {
        }

        public LambdaComparer(Func<T, T, int> lambdaComparer, [AllowNull] Func<T, int> lambdaHash)
        {
            this.lambdaComparer = lambdaComparer;
            this.lambdaHash = lambdaHash;
        }

        public int Compare([AllowNull] T x, [AllowNull] T y)
        {
            return lambdaComparer(x, y);
        }

        public bool Equals([AllowNull] T x, [AllowNull] T y)
        {
            return lambdaComparer(x, y) == 0;
        }

        public int GetHashCode([AllowNull] T obj)
        {
            return lambdaHash != null
                ? lambdaHash(obj)
                : obj?.GetHashCode() ?? 0;
        }
    }
}