/*
Copyright 2012, 2013, 2017 Adam Carter (http://adam-carter.com)

This file is part of FileCache (http://github.com/acarteas/FileCache).

FileCache is distributed under the Apache License 2.0.
Consult "LICENSE.txt" included in this package for the Apache License 2.0.
*/

namespace System.Runtime.Caching
{
    [Serializable]
    public class FileCachePayload
    {
        public object Payload { get; set; }
        public SerializableCacheItemPolicy Policy { get; set; }

        public FileCachePayload(object payload)
        {
            Payload = payload;
            Policy = new SerializableCacheItemPolicy()
            {
                AbsoluteExpiration = DateTime.Now.AddYears(10)
            };
        }

        public FileCachePayload(object payload, SerializableCacheItemPolicy policy)
        {
            Payload = payload;
            Policy = policy;
        }
    }
}