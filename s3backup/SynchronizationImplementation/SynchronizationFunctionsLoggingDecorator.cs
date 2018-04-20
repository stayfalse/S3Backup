using System;
using System.Collections.Generic;
using System.IO;

using S3Backup.Logging;

namespace S3Backup.SynchronizationImplementation
{
    public class SynchronizationFunctionsLoggingDecorator : ISynchronizationFunctions
    {
        private readonly ISynchronizationFunctions _inner;
        private readonly ILog<ISynchronizationFunctions> _log;

        public SynchronizationFunctionsLoggingDecorator(ISynchronizationFunctions inner, ILog<ISynchronizationFunctions> log)
        {
            _inner = inner;
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public Dictionary<string, FileInfo> GetFiles(LocalPath localPath)
        {
            if (localPath is null)
            {
                throw new ArgumentNullException(nameof(localPath));
            }

            var files = _inner.GetFiles(localPath);
            _log.PutOut($"FileInfo dictionary received. (LocalPath: {localPath})");
            return files;
        }

        public bool EqualSize(S3ObjectInfo s3Object, FileInfo fileInfo)
        {
            if (s3Object is null)
            {
                throw new ArgumentNullException(nameof(s3Object));
            }

            if (fileInfo is null)
            {
                throw new ArgumentNullException(nameof(fileInfo));
            }

            _log.PutOut($"Size comparison file {fileInfo.Name} and S3Object {s3Object.Key} started.");
            if (_inner.EqualSize(s3Object, fileInfo))
            {
                _log.PutOut($"Size {s3Object.Key} {fileInfo.Name} matched.");
                return true;
            }

            _log.PutOut($"File {fileInfo.Name} size does not match S3Object {s3Object.Key}.");
            return false;
        }

        public bool EqualETag(S3ObjectInfo s3Object, FileInfo fileInfo, PartSize partSize)
        {
            if (s3Object is null)
            {
                throw new ArgumentNullException(nameof(s3Object));
            }

            if (fileInfo is null)
            {
                throw new ArgumentNullException(nameof(fileInfo));
            }

            _log.PutOut($"Hash comparison file {fileInfo.Name} and S3Object {s3Object.Key} started.");
            if (_inner.EqualETag(s3Object, fileInfo, partSize))
            {
                _log.PutOut($"Hash {s3Object.Key} {fileInfo.Name} matched.");
                return true;
            }

            _log.PutOut($"File {fileInfo.Name} hash does not match S3Object {s3Object.Key}.");
            return false;
        }
    }
}
