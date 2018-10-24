// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;

namespace ReactiveUI.Legacy
{
    internal static class CompatMixins
    {
        internal static void ForEach<T>(this IEnumerable<T> This, Action<T> block)
        {
            foreach (var v in This) {
                block(v);
            }
        }
    }
}
