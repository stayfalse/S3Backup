using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace S3Backup.AmazonS3Functionality
{
    public class AmazonFunctionsDryRunChecker : IAmazonFunctionsDryRunChecker
    {
        private readonly IAmazonFunctions _amazonFunctions;
        private readonly bool _dryRun;

        public AmazonFunctionsDryRunChecker(IAmazonFunctions amazonFunctions, IOptionsSource optionsSource)
        {
            if (optionsSource is null)
            {
                throw new ArgumentNullException(nameof(optionsSource));
            }

            _amazonFunctions = amazonFunctions ?? throw new ArgumentNullException(nameof(amazonFunctions));
            _dryRun = optionsSource.AmazonOptions.DryRun;
        }

        public async Task<bool> TryUploadObjectToBucket(FileInfo fileInfo, LocalPath localPath, PartSize partSize)
        {
            if (!_dryRun)
            {
                await _amazonFunctions
                    .UploadObjectToBucket(fileInfo, localPath, partSize)
                    .ConfigureAwait(false);
            }

            return !_dryRun;
        }

        public async Task<bool> TryUploadObjects(IEnumerable<FileInfo> filesInfo, LocalPath localPath, PartSize partSize)
        {
            if (!_dryRun)
            {
                foreach (var fileInfo in filesInfo)
                {
                    await _amazonFunctions
                        .UploadObjectToBucket(fileInfo, localPath, partSize)
                        .ConfigureAwait(false);
                }
            }

            return !_dryRun;
        }

        public async Task<bool> TryDeleteObject(string objectKey)
        {
            if (!_dryRun)
            {
                await _amazonFunctions.DeleteObject(objectKey).ConfigureAwait(false);
            }

            return !_dryRun;
        }

        public async Task<bool> TryPurge(RemotePath prefix)
        {
            if (!_dryRun)
            {
                await _amazonFunctions.Purge(prefix).ConfigureAwait(false);
            }

            return !_dryRun;
        }
    }
}
