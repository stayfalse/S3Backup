using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace S3Backup
{
    public interface IAmazonFunctions
    {
        Task<IEnumerable<S3ObjectInfo>> GetObjectsList(string prefix);

        Task UploadObjectToBucket(FileInfo file, string localPath, int partSize);

        Task DeleteObject(string key);

        Task Purge(string prefix);
    }
}
