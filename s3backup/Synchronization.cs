using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace S3Backup
{
    public class Synchronization
    {
        private readonly Options _options; // should set only usable options
        private readonly IAmazonFunctions _amazonFunctions;
        private readonly ISynchronizationFunctions _synchronizationFunctions;

        public Synchronization(IOptionsSource optionsSource, IAmazonFunctions amazonFunctions, ISynchronizationFunctions synchronizationFunctions)
        {
            if (optionsSource is null)
            {
                throw new ArgumentNullException(nameof(optionsSource));
            }

            if (amazonFunctions is null)
            {
                throw new ArgumentNullException(nameof(amazonFunctions));
            }

            _options = optionsSource.Options;
            _amazonFunctions = amazonFunctions;
            _synchronizationFunctions = synchronizationFunctions;
        }

        public async Task Synchronize()
        {
            Log.PutOut($"Synchronization started");

            if (_options.Purge && !_options.DryRun)
            {
                await _amazonFunctions.Purge(_options.RemotePath).ConfigureAwait(false);
            }

            var objects = await _amazonFunctions.GetObjectsList(_options.RemotePath).ConfigureAwait(false);
            var filesInfo = _synchronizationFunctions.GetFiles(_options.LocalPath);

            filesInfo = await CompareFilesAndObjectsLists(filesInfo, objects).ConfigureAwait(false);

            await _synchronizationFunctions
                .TryUploadMissingFiles(filesInfo, _options.DryRun, _options.LocalPath, _options.PartSize)
                .ConfigureAwait(false);

            Log.PutOut($"{(_options.DryRun ? "DryRun" : "")} Synchronization completed");
        }

        private async Task<Dictionary<string, FileInfo>> CompareFilesAndObjectsLists(Dictionary<string, FileInfo> filesInfo, IEnumerable<S3ObjectInfo> objects)
        {
            foreach (var s3Object in objects)
            {
                if (filesInfo.TryGetValue(s3Object.Key, out var fileInfo))
                {
                    filesInfo.Remove(fileInfo.Name);
                    if (CompareFileAndObject(fileInfo, s3Object))
                    {
                        continue;
                    }
                    else
                    {
                        await _synchronizationFunctions
                            .TryUploadMismatchedFile(fileInfo, _options.DryRun, _options.LocalPath, _options.PartSize)
                            .ConfigureAwait(false);
                    }
                }
                else
                {
                    await _synchronizationFunctions.TryDeleteMismatchedObject(s3Object, _options.DryRun, _options.RecycleAge).ConfigureAwait(false);
                }
            }

            return filesInfo;
        }

        private bool CompareFileAndObject(FileInfo fileInfo, S3ObjectInfo s3Object)
        {
            Log.PutOut($"Comparation object {s3Object.Key} and file {fileInfo.Name} started");

            if (_synchronizationFunctions.EqualSize(s3Object, fileInfo))
            {
                if (_options.SizeOnly)
                {
                    return true;
                }
                else
                {
                    if (_synchronizationFunctions.EqualETag(s3Object, fileInfo, _options.PartSize))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }
        }
    }
}
