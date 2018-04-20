using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace S3Backup.AmazonS3Functionality
{
    public class AmazonFunctionsForSynchronization : IAmazonFunctionsForSynchronization
    {
        private readonly IAmazonFunctionsDryRunChecker _dryRun;
        private readonly IAmazonFunctions _functions;

        public AmazonFunctionsForSynchronization(IAmazonFunctionsDryRunChecker amazonFunctionsDryRunChecker, IAmazonFunctions functions)
        {
            _dryRun = amazonFunctionsDryRunChecker ?? throw new ArgumentNullException(nameof(amazonFunctionsDryRunChecker));
        }

        public async Task<IEnumerable<S3ObjectInfo>> GetObjectsList(RemotePath prefix)
        {
            return await _functions.GetObjectsList(prefix).ConfigureAwait(false);
        }

        public async Task DeleteObject(string objectKey)
        {
            await _dryRun.TryDeleteObject(objectKey).ConfigureAwait(false);
        }

        public async Task UploadObjects(IEnumerable<FileInfo> filesInfo, LocalPath localPath, PartSize partSize)
        {
            await _dryRun.TryUploadObjects(filesInfo, localPath, partSize).ConfigureAwait(false);
        }

        public async Task Purge(RemotePath prefix)
        {
            await _dryRun.TryPurge(prefix).ConfigureAwait(false);
        }

        public async Task UploadObjectToBucket(FileInfo fileInfo, LocalPath localPath, PartSize partSize)
        {
            await _dryRun.TryUploadObjectToBucket(fileInfo, localPath, partSize).ConfigureAwait(false);
        }
    }
}
