using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace S3Backup
{
    public class SynchonizationLoggingDecorator : ISynchronization
    {
        private readonly ISynchronization _inner;

        public SynchonizationLoggingDecorator(ISynchronization synchronization)
        {
            _inner = synchronization ?? throw new ArgumentNullException(nameof(synchronization));
        }

        public async Task Synchronize()
        {
            Log.PutOut($"Synchronization started");
            await _inner.Synchronize().ConfigureAwait(false);
            Log.PutOut($"Synchronization completed");
        }

        public async Task<IEnumerable<FileInfo>> CompareLocalFilesAndS3Objects(IEnumerable<S3ObjectInfo> objects)
        {
            Log.PutOut($"Comparison local files and objects from bucket started.");
            return await _inner.CompareLocalFilesAndS3Objects(objects).ConfigureAwait(false);
        }

        public bool FileEqualsObject(FileInfo fileInfo, S3ObjectInfo s3Object)
        {
            Log.PutOut($"Comparison file {fileInfo.Name} and S3Object {s3Object.Key} started.");
            return _inner.FileEqualsObject(fileInfo, s3Object);
        }
    }
}
