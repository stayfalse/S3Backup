using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace S3Backup
{
    public interface IAmazonFunctionsDryRunChecker
    {
        Task<IEnumerable<S3ObjectInfo>> GetObjectsList(RemotePath prefix);

        Task<bool> TryDeleteObject(string key);

        Task<bool> TryUploadObjects(IEnumerable<FileInfo> filesInfo, LocalPath localPath, PartSize partSize);

        Task<bool> TryPurge(RemotePath prefix);

        Task<bool> TryUploadObjectToBucket(FileInfo fileInfo, LocalPath localPath, PartSize partSize);
    }
}
