using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace S3Backup.AmazonS3Functionality
{
    public interface IAmazonFunctionsForSynchronization
    {
        Task DeleteObject(string objectKey);

        Task<IEnumerable<S3ObjectInfo>> GetObjectsList(RemotePath prefix);

        Task Purge(RemotePath prefix);

        Task UploadObjects(IEnumerable<FileInfo> filesInfo, ObjectKeyCreator keyCreator, PartSize partSize);

        Task UploadObjectToBucket(FileInfo fileInfo, ObjectKeyCreator keyCreator, PartSize partSize);
    }
}
