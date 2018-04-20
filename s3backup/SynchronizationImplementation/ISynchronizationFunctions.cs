using System.Collections.Generic;
using System.IO;

namespace S3Backup.SynchronizationImplementation
{
    public interface ISynchronizationFunctions
    {
        Dictionary<string, FileInfo> GetFiles(LocalPath localPath);

        bool EqualSize(S3ObjectInfo s3Object, FileInfo fileInfo);

        bool EqualETag(S3ObjectInfo s3Object, FileInfo fileInfo, PartSize partSize);
    }
}
