using System;
using NSubstitute;

public static class LazySubstitute
{
    public static Lazy<T> For<T>(params object[] constructorArguments) where T : class
    {
        return new Lazy<T>(() => Substitute.For<T>(constructorArguments));
    }
}
