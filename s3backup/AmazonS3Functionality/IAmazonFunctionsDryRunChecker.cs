using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace S3Backup.AmazonS3Functionality
{
    public interface IAmazonFunctionsDryRunChecker
    {
        Task<bool> TryDeleteObject(string objectKey);

        Task<bool> TryUploadObjects(IEnumerable<FileInfo> filesInfo, LocalPath localPath, PartSize partSize);

        Task<bool> TryPurge(RemotePath prefix);

        Task<bool> TryUploadObjectToBucket(FileInfo fileInfo, LocalPath localPath, PartSize partSize);
    }
}
