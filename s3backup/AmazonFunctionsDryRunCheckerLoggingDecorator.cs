using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace S3Backup
{
    public class AmazonFunctionsDryRunCheckerLoggingDecorator : IAmazonFunctionsDryRunChecker
    {
        private readonly IAmazonFunctionsDryRunChecker _inner;

        public AmazonFunctionsDryRunCheckerLoggingDecorator(IAmazonFunctionsDryRunChecker amazonFunctionsDryRunChecker)
        {
            _inner = amazonFunctionsDryRunChecker ?? throw new ArgumentNullException(nameof(amazonFunctionsDryRunChecker));
        }

        public async Task<IEnumerable<S3ObjectInfo>> GetObjectsList(RemotePath prefix)
        {
            var objectsList = _inner.GetObjectsList(prefix);
            Log.PutOut($"S3Objects list received. (RemotePath: {prefix})");
            return await objectsList.ConfigureAwait(false);
        }

        public async Task<bool> TryUploadObjectToBucket(FileInfo fileInfo, LocalPath localPath, PartSize partSize)
        {
            if (!await _inner.TryUploadObjectToBucket(fileInfo, localPath, partSize).ConfigureAwait(false))
            {
                Log.PutOut($"{fileInfo.Name} upload skipped.");
                return false;
            }

            return true;
        }

        public async Task<bool> TryUploadObjects(IEnumerable<FileInfo> filesInfo, LocalPath localPath, PartSize partSize)
        {
            Log.PutOut($"Upload multiple objects.");
            foreach (var fileInfo in filesInfo)
            {
                if (!await _inner.TryUploadObjectToBucket(fileInfo, localPath, partSize).ConfigureAwait(false))
                {
                    Log.PutOut($"Multiple upload skipped.");
                    return false;
                }
            }

            Log.PutOut($"Multiple objects uploaded.");
            return true;
        }

        public async Task<bool> TryDeleteObject(string key)
        {
            if (!await _inner.TryDeleteObject(key).ConfigureAwait(false))
            {
                Log.PutOut($"{key} object deletion skipped.");
                return false;
            }

            return true;
        }

        public async Task<bool> TryPurge(RemotePath prefix)
        {
            if (!await _inner.TryPurge(prefix).ConfigureAwait(false))
            {
                Log.PutOut($"Purge skipped.");
                return false;
            }

            return true;
        }
    }
}
