﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace S3Backup.AmazonS3Functionality
{
    public class AmazonFunctionsForSynchronization : IAmazonFunctionsForSynchronization
    {
        private readonly IAmazonFunctionsDryRunChecker _dryRun;
        private readonly IAmazonFunctions _functions;

        public AmazonFunctionsForSynchronization(IAmazonFunctionsDryRunChecker amazonFunctionsDryRunChecker, IAmazonFunctions amazonFunctions)
        {
            _dryRun = amazonFunctionsDryRunChecker ?? throw new ArgumentNullException(nameof(amazonFunctionsDryRunChecker));
            _functions = amazonFunctions ?? throw new ArgumentNullException(nameof(amazonFunctions));
        }

        public async Task<IEnumerable<S3ObjectInfo>> GetObjectsList(RemotePath prefix)
        {
            return await _functions.GetObjectsList(prefix).ConfigureAwait(false);
        }

        public async Task DeleteObject(string objectKey)
        {
            await _dryRun.TryDeleteObject(objectKey).ConfigureAwait(false);
        }

        public async Task UploadObjects(IEnumerable<FileInfo> filesInfo, ObjectKeyCreator keyCreator, PartSize partSize)
        {
            await _dryRun.TryUploadObjects(filesInfo, keyCreator, partSize).ConfigureAwait(false);
        }

        public async Task Purge(RemotePath prefix)
        {
            await _dryRun.TryPurge(prefix).ConfigureAwait(false);
        }

        public async Task UploadObjectToBucket(FileInfo fileInfo, ObjectKeyCreator keyCreator, PartSize partSize)
        {
            await _dryRun.TryUploadObjectToBucket(fileInfo, keyCreator, partSize).ConfigureAwait(false);
        }
    }
}
