using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace S3Backup
{
    public interface ISynchronizationFunctions
    {
        Dictionary<string, FileInfo> GetFiles();

        Task<bool> TryUploadMissingFiles(Dictionary<string, FileInfo> filesInfo);

        Task<bool> TryUploadMismatchedFile(FileInfo fileInfo);

        Task<bool> TryDeleteMismatchedObject(S3ObjectInfo s3Object);

        bool EqualSize(S3ObjectInfo s3Object, FileInfo fileInfo);

        bool EqualETag(S3ObjectInfo s3Object, FileInfo fileInfo);
    }
}
