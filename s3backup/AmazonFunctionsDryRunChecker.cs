﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace S3Backup
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
            _dryRun = (optionsSource.Options.OptionCases & OptionCases.DryRun) == OptionCases.DryRun;
        }

        public async Task<IEnumerable<S3ObjectInfo>> GetObjectsList(RemotePath prefix)
        {
            return await _amazonFunctions.GetObjectsList(prefix).ConfigureAwait(false);
        }

        public async Task<bool> TryUploadObjectToBucket(FileInfo fileInfo, LocalPath localPath, PartSize partSize)
        {
            if (!_dryRun)
            {
                await _amazonFunctions.UploadObjectToBucket(fileInfo, localPath, partSize).ConfigureAwait(false);
            }

            return !_dryRun;
        }

        public async Task<bool> TryUploadObjects(ICollection<FileInfo> filesInfo, LocalPath localPath, PartSize partSize)
        {
            if (!_dryRun)
            {
                await _amazonFunctions.UploadObjects(filesInfo, localPath, partSize).ConfigureAwait(false);
            }

            return !_dryRun;
        }

        public async Task<bool> TryDeleteObject(string key)
        {
            if (!_dryRun)
            {
                await _amazonFunctions.DeleteObject(key).ConfigureAwait(false);
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