using System;

namespace S3Backup
{
    public interface IS3Object
    {
        string Key { get; }

        long Size { get; }

        string ETag { get; }

        DateTime LastModified { get; }
    }
}
