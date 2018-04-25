using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace S3Backup.SynchronizationImplementation
{
    public interface ISynchronizationFunctions
    {
        Task DeleteExcessObject(S3ObjectInfo s3Object);

        bool FileEqualsObject(FileInfo fileInfo, S3ObjectInfo s3Object);

        Dictionary<string, FileInfo> GetFilesDictionary();

        Task<IEnumerable<S3ObjectInfo>> GetObjectsList();

        Task Purge();

        Task UploadMismatchedFile(FileInfo fileInfo);

        Task UploadMissingFiles(IReadOnlyCollection<FileInfo> filesInfo);
    }
}
