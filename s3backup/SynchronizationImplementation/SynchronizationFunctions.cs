using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using S3Backup.AmazonS3Functionality;

namespace S3Backup.SynchronizationImplementation
{
    public class SynchronizationFunctions : ISynchronizationFunctions
    {
        private readonly Options _options;
        private readonly IAmazonFunctionsForSynchronization _amazonFunctions;
        private readonly IComparisonFunctions _comparisonFunctions;

        public SynchronizationFunctions(IOptionsSource optionsSource, IAmazonFunctionsForSynchronization amazonFunctions, IComparisonFunctions comparisonFunctions)
        {
            if (optionsSource is null)
            {
                throw new ArgumentNullException(nameof(optionsSource));
            }

            _options = optionsSource.Options;
            _amazonFunctions = amazonFunctions ?? throw new ArgumentNullException(nameof(amazonFunctions));
            _comparisonFunctions = comparisonFunctions ?? throw new ArgumentNullException(nameof(comparisonFunctions));
        }

        public async Task Purge()
        {
            if ((_options.OptionCases & OptionCases.Purge) == OptionCases.Purge)
            {
                await _amazonFunctions.Purge(_options.RemotePath).ConfigureAwait(false);
            }
        }

        public async Task<IEnumerable<S3ObjectInfo>> GetObjectsList()
        {
            return await _amazonFunctions.GetObjectsList(_options.RemotePath).ConfigureAwait(false);
        }

        public Dictionary<string, FileInfo> GetFilesDictionary()
        {
            var files = new DirectoryInfo(_options.LocalPath)
                .GetFiles("*", SearchOption.AllDirectories);
            var filesInfo = new Dictionary<string, FileInfo>();
            foreach (var fileInfo in files)
            {
                filesInfo.Add(fileInfo.FullName.Remove(0, Path.GetFullPath(_options.LocalPath).Length + 1).Replace('\\', '/'), fileInfo);
            }

            return filesInfo;
        }

        public bool FileEqualsObject(FileInfo fileInfo, S3ObjectInfo s3Object)
        {
            if (_comparisonFunctions.EqualSize(s3Object, fileInfo))
            {
                if ((_options.OptionCases & OptionCases.SizeOnly) != OptionCases.SizeOnly)
                {
                    return _comparisonFunctions.EqualETag(s3Object, fileInfo, _options.PartSize);
                }

                return true;
            }

            return false;
        }

        public async Task DeleteExcessObject(S3ObjectInfo s3Object)
        {
            if (s3Object is null)
            {
                throw new ArgumentNullException(nameof(s3Object));
            }

            if (s3Object.LastModified < _options.ThresholdDate)
            {
                await _amazonFunctions.DeleteObject(s3Object.Key).ConfigureAwait(false);
            }
        }

        public async Task UploadMismatchedFile(FileInfo fileInfo)
        {
            await _amazonFunctions
                           .UploadObjectToBucket(fileInfo, _options.LocalPath, _options.PartSize)
                           .ConfigureAwait(false);
        }

        public async Task UploadMissingFiles(IReadOnlyCollection<FileInfo> filesInfo)
        {
            if (filesInfo is null)
            {
                throw new ArgumentNullException(nameof(filesInfo));
            }

            if (filesInfo.Count > 0)
            {
                await _amazonFunctions
                  .UploadObjects(filesInfo, _options.LocalPath, _options.PartSize)
                  .ConfigureAwait(false);
            }
        }
    }
}
