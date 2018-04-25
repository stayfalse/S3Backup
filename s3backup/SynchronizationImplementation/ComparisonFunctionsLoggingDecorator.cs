using System;
using System.IO;

using S3Backup.Logging;

namespace S3Backup.SynchronizationImplementation
{
    public class ComparisonFunctionsLoggingDecorator : IComparisonFunctions
    {
        private readonly IComparisonFunctions _inner;
        private readonly ILog<IComparisonFunctions> _log;

        public ComparisonFunctionsLoggingDecorator(IComparisonFunctions comparisonFunctions, ILog<IComparisonFunctions> log)
        {
            _inner = comparisonFunctions ?? throw new ArgumentNullException(nameof(comparisonFunctions));
            _log = log ?? throw new ArgumentNullException(nameof(log));
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

            if (_inner.EqualSize(s3Object, fileInfo))
            {
                _log.PutOut($"Size {s3Object.Key} {fileInfo.Name} matched.");
                return true;
            }

            _log.PutOut($"File {fileInfo.Name} size does not match S3Object {s3Object.Key} size.");
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

            if (_inner.EqualETag(s3Object, fileInfo, partSize))
            {
                _log.PutOut($"Hash {s3Object.Key} {fileInfo.Name} matched.");
                return true;
            }

            _log.PutOut($"File {fileInfo.Name} hash does not match S3Object {s3Object.Key} hash.");
            return false;
        }
    }
}
