using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

using S3Backup.Logging;

namespace S3Backup.AmazonS3Functionality
{
    public class AmazonFunctionsLoggingDecorator : IAmazonFunctions
    {
        private readonly IAmazonFunctions _inner;
        private readonly ILog<IAmazonFunctions> _log;

        public AmazonFunctionsLoggingDecorator(IAmazonFunctions amazonFunctions, ILog<IAmazonFunctions> log)
        {
            _inner = amazonFunctions ?? throw new ArgumentNullException(nameof(amazonFunctions));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public async Task<IEnumerable<S3ObjectInfo>> GetObjectsList(RemotePath prefix)
        {
            if (prefix is null)
            {
                throw new ArgumentNullException(nameof(prefix));
            }

            var objectsList = await _inner.GetObjectsList(prefix).ConfigureAwait(false);
            _log.PutOut($"S3Objects list received. (RemotePath: {prefix.ToString(CultureInfo.InvariantCulture)})");
            return objectsList;
        }

        public async Task UploadObjectToBucket(FileInfo fileInfo, ObjectKeyCreator keyCreator, PartSize partSize)
        {
            if (fileInfo is null)
            {
                throw new ArgumentNullException(nameof(fileInfo));
            }

            if (keyCreator is null)
            {
                throw new ArgumentNullException(nameof(keyCreator));
            }

            _log.PutOut($"Upload {keyCreator(fileInfo.FullName)} to bucket started.");
            await _inner.UploadObjectToBucket(fileInfo, keyCreator, partSize).ConfigureAwait(false);
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
