using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace S3Backup
{
    public class AmazonFunctionsLoggingDecorator : IAmazonFunctions
    {
        private readonly IAmazonFunctions _amazonFunctions;

        public AmazonFunctionsLoggingDecorator(IAmazonFunctions amazonFunctions)
        {
            _amazonFunctions = amazonFunctions ?? throw new ArgumentNullException(nameof(amazonFunctions));
        }

        public async Task<IEnumerable<S3ObjectInfo>> GetObjectsList(RemotePath prefix)
        {
            Log.PutOut($"Receive AmazonS3ObjectsList (RemotePath: {prefix})");
            return await _amazonFunctions.GetObjectsList(prefix).ConfigureAwait(false);
        }

        public async Task UploadObjectToBucket(FileInfo file, LocalPath localPath, PartSize partSize)
        {
            Log.PutOut($"Upload {file.Name} to bucket");
            await _amazonFunctions.UploadObjectToBucket(file, localPath, partSize).ConfigureAwait(false);
            Log.PutOut($"Uploaded");
        }

        public async Task DeleteObject(string key)
        {
            Log.PutOut($"Delete object {key}");
            await _amazonFunctions.DeleteObject(key).ConfigureAwait(false);
        }

        public async Task Purge(RemotePath prefix)
        {
            Log.PutOut($"Purge bucket with remote path {prefix}");
            await _amazonFunctions.Purge(prefix).ConfigureAwait(false);
        }
    }
}
