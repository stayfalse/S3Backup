using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using S3Backup.Logging;

namespace S3Backup.AmazonS3Functionality
{
    public class AmazonFunctionsDryRunCheckerLoggingDecorator : IAmazonFunctionsDryRunChecker
    {
        private readonly IAmazonFunctionsDryRunChecker _inner;
        private readonly ILog<IAmazonFunctionsDryRunChecker> _log;

        public AmazonFunctionsDryRunCheckerLoggingDecorator(IAmazonFunctionsDryRunChecker amazonFunctionsDryRunChecker, ILog<IAmazonFunctionsDryRunChecker> log)
        {
            _inner = amazonFunctionsDryRunChecker ?? throw new ArgumentNullException(nameof(amazonFunctionsDryRunChecker));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public async Task<bool> TryUploadObjectToBucket(FileInfo fileInfo, LocalPath localPath, PartSize partSize)
        {
            if (fileInfo is null)
            {
                throw new ArgumentNullException(nameof(fileInfo));
            }

            _log.PutOut($"Try upload multiple missing object.");
            if (!await _inner.TryUploadObjectToBucket(fileInfo, localPath, partSize).ConfigureAwait(false))
            {
                _log.PutOut($"{fileInfo.Name} upload skipped.");
                return false;
            }

            return true;
        }

        public async Task<bool> TryUploadObjects(IEnumerable<FileInfo> filesInfo, LocalPath localPath, PartSize partSize)
        {
            _log.PutOut($"Try upload multiple missing objects.");
            foreach (var fileInfo in filesInfo)
            {
                if (!await _inner.TryUploadObjectToBucket(fileInfo, localPath, partSize).ConfigureAwait(false))
                {
                    _log.PutOut($"Multiple upload skipped.");
                    return false;
                }
            }

            return true;
        }

        public async Task<bool> TryDeleteObject(string objectKey)
        {
            _log.PutOut($"Try delete mismatching object {objectKey}.");
            if (!await _inner.TryDeleteObject(objectKey).ConfigureAwait(false))
            {
                _log.PutOut($"{objectKey} object deletion skipped.");
                return false;
            }

            return true;
        }

        public async Task<bool> TryPurge(RemotePath prefix)
        {
            if (!await _inner.TryPurge(prefix).ConfigureAwait(false))
            {
                _log.PutOut($"Purge skipped.");
                return false;
            }

            return true;
        }
    }
}
