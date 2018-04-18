using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace S3Backup
{
    public class SynchonizationLoggingDecorator : ISynchronization
    {
        private readonly ISynchronization _inner;
        private readonly ILog<ISynchronization> _log;

        public SynchonizationLoggingDecorator(ISynchronization synchronization, ILog<ISynchronization> log)
        {
            _inner = synchronization ?? throw new ArgumentNullException(nameof(synchronization));
            _log = log;
        }

        public async Task Synchronize()
        {
            _log.PutOut($"Synchronization started");
            await _inner.Synchronize().ConfigureAwait(false);
            _log.PutOut($"Synchronization completed");
        }

        public async Task<IEnumerable<FileInfo>> CompareLocalFilesAndS3Objects(IEnumerable<S3ObjectInfo> objects)
        {
            _log.PutOut($"Comparison local files and objects from bucket started.");
            return await _inner.CompareLocalFilesAndS3Objects(objects).ConfigureAwait(false);
        }

        public bool FileEqualsObject(FileInfo fileInfo, S3ObjectInfo s3Object)
        {
            _log.PutOut($"Comparison file {fileInfo.Name} and S3Object {s3Object.Key} started.");
            return _inner.FileEqualsObject(fileInfo, s3Object);
        }
    }
}
