using System;
using System.Collections.Generic;

namespace GitHub.Models
{
    public class Page<T>
    {
        public string EndCursor { get; set; }
        public bool HasNextPage { get; set; }
        public int TotalCount { get; set; }
        public IList<T> Items { get; set; }
    }
}
