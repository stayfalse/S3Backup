using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using S3Backup.AmazonS3Functionality;

namespace S3Backup.SynchronizationImplementation
{
    public class Synchronization : ISynchronization
    {
        private readonly Options _options;
        private readonly IAmazonFunctionsForSynchronization _amazonFunctions;
        private readonly ISynchronizationFunctions _synchronizationFunctions;

        public Synchronization(IOptionsSource optionsSource, IAmazonFunctionsForSynchronization amazonFunctions, ISynchronizationFunctions synchronizationFunctions)
        {
            if (optionsSource is null)
            {
                throw new ArgumentNullException(nameof(optionsSource));
            }

            _options = optionsSource.Options;
            _amazonFunctions = amazonFunctions ?? throw new ArgumentNullException(nameof(amazonFunctions));
            _synchronizationFunctions = synchronizationFunctions ?? throw new ArgumentNullException(nameof(synchronizationFunctions));
        }

        public async Task Synchronize()
        {
            if ((_options.OptionCases & OptionCases.Purge) != OptionCases.Purge)
            {
                await _amazonFunctions.Purge(_options.RemotePath).ConfigureAwait(false);
            }

            var objects = await _amazonFunctions.GetObjectsList(_options.RemotePath).ConfigureAwait(false);

            var filesInfo = await CompareLocalFilesAndS3Objects(objects).ConfigureAwait(false);

            await _amazonFunctions
                .UploadObjects(filesInfo, _options.LocalPath, _options.PartSize)
                .ConfigureAwait(false);
        }

        private async Task<IEnumerable<FileInfo>> CompareLocalFilesAndS3Objects(IEnumerable<S3ObjectInfo> objects)
        {
            if (objects is null)
            {
                throw new ArgumentNullException(nameof(objects));
            }

            var filesInfo = _synchronizationFunctions.GetFiles(_options.LocalPath);
            foreach (var s3Object in objects)
            {
                if (filesInfo.TryGetValue(s3Object.Key, out var fileInfo))
                {
                    filesInfo.Remove(fileInfo.Name);
                    if (!FileEqualsObject(fileInfo, s3Object))
                    {
                        await _amazonFunctions
                           .UploadObjectToBucket(fileInfo, _options.LocalPath, _options.PartSize)
                           .ConfigureAwait(false);
                    }
                }
                else
                {
                    if (s3Object.LastModified < _options.ThresholdDate)
                    {
                        await _amazonFunctions.DeleteObject(s3Object.Key).ConfigureAwait(false);
                    }
                }
            }

            return filesInfo.Values;
        }

        private bool FileEqualsObject(FileInfo fileInfo, S3ObjectInfo s3Object)
        {
            if (_synchronizationFunctions.EqualSize(s3Object, fileInfo))
            {
                if ((_options.OptionCases & OptionCases.SizeOnly) != OptionCases.SizeOnly)
                {
                    return _synchronizationFunctions.EqualETag(s3Object, fileInfo, _options.PartSize);
                }

                return true;
            }

            return false;
        }
    }
}
