using System;

namespace S3Backup
{
    public sealed class S3ObjectInfo
    {
        public S3ObjectInfo(string key, long size, string eTag, DateTime lastModified)
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
            Size = size;
            ETag = eTag ?? throw new ArgumentNullException(nameof(eTag));
            LastModified = lastModified;
        }

        public string Key { get; }

        public long Size { get; }

        public string ETag { get; }

        public DateTime LastModified { get; }
    }
}
