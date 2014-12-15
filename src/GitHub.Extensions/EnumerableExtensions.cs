using System;
using System.Collections.Generic;
using System.Linq;

namespace GitHub
{
    public static class EnumerableExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source) action(item);
        }

        public static IEnumerable<TSource> Except<TSource>(
            this IEnumerable<TSource> enumerable,
            IEnumerable<TSource> second,
            Func<TSource, TSource, bool> comparer)
        {
            return enumerable.Except(second, new LambdaComparer<TSource>(comparer));
        }

        class LambdaComparer<T> : IEqualityComparer<T>
        {
            private readonly Func<T, T, bool> lambdaComparer;
            private readonly Func<T, int> lambdaHash;

            public LambdaComparer(Func<T, T, bool> lambdaComparer) :
                this(lambdaComparer, o => 0)
            {
            }

            LambdaComparer(Func<T, T, bool> lambdaComparer, Func<T, int> lambdaHash)
            {
                this.lambdaComparer = lambdaComparer;
                this.lambdaHash = lambdaHash;
            }

            public bool Equals(T x, T y)
            {
                return lambdaComparer(x, y);
            }

            public int GetHashCode(T obj)
            {
                return lambdaHash(obj);
            }
        }

    }
}
