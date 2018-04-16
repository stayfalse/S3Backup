using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace S3Backup
{
    public interface IAmazonFunctionsForSynchronization
    {
        Task DeleteObject(string objectKey);

        Task<IEnumerable<S3ObjectInfo>> GetObjectsList(RemotePath prefix);

        Task Purge(RemotePath prefix);

        Task UploadObjects(IEnumerable<FileInfo> filesInfo, LocalPath localPath, PartSize partSize);

        Task UploadObjectToBucket(FileInfo fileInfo, LocalPath localPath, PartSize partSize);
    }
}
