using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace S3Backup
{
    public class AmazonFunctionsLoggingDecorator : IAmazonFunctions
    {
        private readonly IAmazonFunctions _inner;
        private readonly ILog<IAmazonFunctions> _log;

        public AmazonFunctionsLoggingDecorator(IAmazonFunctions amazonFunctionsDryRun, ILog<IAmazonFunctions> log)
        {
            _inner = amazonFunctionsDryRun ?? throw new ArgumentNullException(nameof(amazonFunctionsDryRun));
            _log = log ?? throw new ArgumentNullException(nameof(log));
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

            _log.PutOut($"Upload {fileInfo.Name} to bucket started.");
            await _inner.UploadObjectToBucket(fileInfo, localPath, partSize).ConfigureAwait(false);
            _log.PutOut($"Uploaded");
        }

        public async Task DeleteObject(string objectKey)
        {
            _log.PutOut($"Delete object {objectKey}.");
            await _inner.DeleteObject(objectKey).ConfigureAwait(false);
            _log.PutOut($"Deleted.");
        }

        public async Task Purge(RemotePath prefix)
        {
            if (prefix is null)
            {
                throw new ArgumentNullException(nameof(prefix));
            }

            _log.PutOut($"Purge bucket with remote path {prefix}");
            await _inner.Purge(prefix).ConfigureAwait(false);
            _log.PutOut($"Purge completed.");
        }
    }
}
