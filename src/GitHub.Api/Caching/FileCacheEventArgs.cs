/*
Copyright 2012, 2013, 2017 Adam Carter (http://adam-carter.com)

This file is part of FileCache (http://github.com/acarteas/FileCache).

FileCache is distributed under the Apache License 2.0.
Consult "LICENSE.txt" included in this package for the Apache License 2.0.
*/

namespace System.Runtime.Caching
{
    public class FileCacheEventArgs : EventArgs
    {
        public long CurrentCacheSize { get; private set; }
        public long MaxCacheSize { get; private set; }
        public FileCacheEventArgs(long currentSize, long maxSize)
        {
            CurrentCacheSize = currentSize;
            MaxCacheSize = maxSize;
        }
    }
}