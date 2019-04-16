/*
Copyright 2012, 2013, 2017 Adam Carter (http://adam-carter.com)

This file is part of FileCache (http://github.com/acarteas/FileCache).

FileCache is distributed under the Apache License 2.0.
Consult "LICENSE.txt" included in this package for the Apache License 2.0.
*/

namespace System.Runtime.Caching
{
    [Serializable]
    public class SerializableCacheItemPolicy
    {
        public DateTimeOffset AbsoluteExpiration { get; set; }

        private TimeSpan _slidingExpiration;
        public TimeSpan SlidingExpiration
        {
            get
            {
                return _slidingExpiration;
            }
            set
            {
                _slidingExpiration = value;
                if (_slidingExpiration > new TimeSpan())
                {
                    AbsoluteExpiration = DateTimeOffset.Now.Add(_slidingExpiration);
                }
            }
        }
        public SerializableCacheItemPolicy(CacheItemPolicy policy)
        {
            AbsoluteExpiration = policy.AbsoluteExpiration;
            SlidingExpiration = policy.SlidingExpiration;
        }

        public SerializableCacheItemPolicy()
        {
            SlidingExpiration = new TimeSpan();
        }
    }
}