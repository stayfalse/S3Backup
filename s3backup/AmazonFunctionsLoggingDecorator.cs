using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace S3Backup
{
    public class AmazonFunctionsLoggingDecorator : IAmazonFunctions
    {
        private readonly IAmazonFunctions _inner;

        public AmazonFunctionsLoggingDecorator(IAmazonFunctions amazonFunctionsDryRun)
        {
            _inner = amazonFunctionsDryRun ?? throw new ArgumentNullException(nameof(amazonFunctionsDryRun));
        }

        public async Task<IEnumerable<S3ObjectInfo>> GetObjectsList(RemotePath prefix)
        {
            return await _inner.GetObjectsList(prefix).ConfigureAwait(false);
        }

        public async Task UploadObjectToBucket(FileInfo fileInfo, LocalPath localPath, PartSize partSize)
        {
            if (fileInfo is null)
            {
                throw new ArgumentNullException(nameof(fileInfo));
            }

            Log.PutOut($"Upload {fileInfo.Name} to bucket started.");
            await _inner.UploadObjectToBucket(fileInfo, localPath, partSize).ConfigureAwait(false);
            Log.PutOut($"Uploaded");
        }

        public async Task DeleteObject(string objectKey)
        {
            Log.PutOut($"Delete object {objectKey}.");
            await _inner.DeleteObject(objectKey).ConfigureAwait(false);
            Log.PutOut($"Deleted.");
        }

        public async Task Purge(RemotePath prefix)
        {
            if (prefix is null)
            {
                throw new ArgumentNullException(nameof(prefix));
            }

            Log.PutOut($"Purge bucket with remote path {prefix}");
            await _inner.Purge(prefix).ConfigureAwait(false);
            Log.PutOut($"Purge completed.");
        }
    }
}
