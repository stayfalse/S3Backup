using System.IO;

namespace S3Backup.SynchronizationImplementation
{
    public interface IComparisonFunctions
    {
        bool EqualETag(S3ObjectInfo s3Object, FileInfo fileInfo, PartSize partSize);

        bool EqualSize(S3ObjectInfo s3Object, FileInfo fileInfo);
    }
}
