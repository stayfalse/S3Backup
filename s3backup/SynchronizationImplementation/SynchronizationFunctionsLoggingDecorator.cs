using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using S3Backup.Logging;

namespace S3Backup.SynchronizationImplementation
{
    public class SynchronizationFunctionsLoggingDecorator : ISynchronizationFunctions
    {
        private readonly ISynchronizationFunctions _inner;
        private readonly ILog<ISynchronizationFunctions> _log;

        public SynchronizationFunctionsLoggingDecorator(ISynchronizationFunctions synchronizationFunctions, ILog<ISynchronizationFunctions> log)
        {
            _inner = synchronizationFunctions ?? throw new ArgumentNullException(nameof(synchronizationFunctions));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public Dictionary<string, FileInfo> GetFilesDictionary()
        {
            _log.PutOut($"Try to receive files dictionary.");
            var files = _inner.GetFilesDictionary();
            _log.PutOut($"FileInfo dictionary received.");
            return files;
        }

        public async Task DeleteExcessObject(S3ObjectInfo s3Object)
        {
            if (s3Object is null)
            {
                throw new ArgumentNullException(nameof(s3Object));
            }

            _log.PutOut($"Local directory does not contain file {s3Object.Key}.");
            await _inner.DeleteExcessObject(s3Object).ConfigureAwait(false);
        }

        public bool FileEqualsObject(FileInfo fileInfo, S3ObjectInfo s3Object)
        {
            if (s3Object is null)
            {
                throw new ArgumentNullException(nameof(s3Object));
            }

            if (fileInfo is null)
            {
                throw new ArgumentNullException(nameof(fileInfo));
            }

            _log.PutOut($"Comparison of file {fileInfo.Name} and S3Object {s3Object.Key} started.");
            if (_inner.FileEqualsObject(fileInfo, s3Object))
            {
                _log.PutOut($"File {fileInfo.Name} matches object {s3Object.Key}.");
                return true;
            }

            return false;
        }

        public async Task<IEnumerable<S3ObjectInfo>> GetObjectsList()
        {
            _log.PutOut($"Try to receive S3Objects list.");
            return await _inner.GetObjectsList().ConfigureAwait(false);
        }

        public async Task Purge()
        {
            await _inner.Purge().ConfigureAwait(false);
        }

        public async Task UploadMismatchedFile(FileInfo fileInfo)
        {
            if (fileInfo is null)
            {
                throw new ArgumentNullException(nameof(fileInfo));
            }

            _log.PutOut($"File {fileInfo.Name} does not match object.");
            await _inner.UploadMismatchedFile(fileInfo).ConfigureAwait(false);
        }

        public async Task UploadMissingFiles(IReadOnlyCollection<FileInfo> filesInfo)
        {
            if (filesInfo is null)
            {
                throw new ArgumentNullException(nameof(filesInfo));
            }

            _log.PutOut($"{filesInfo.Count} files are missing.");
            await _inner.UploadMissingFiles(filesInfo).ConfigureAwait(false);
        }
    }
}
