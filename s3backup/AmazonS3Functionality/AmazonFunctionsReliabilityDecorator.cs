﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using S3Backup.Logging;

namespace S3Backup.AmazonS3Functionality
{
    public class AmazonFunctionsReliabilityDecorator : IAmazonFunctions
    {
        private readonly IAmazonFunctions _inner;
        private readonly ILog<IAmazonFunctions> _log;

        public AmazonFunctionsReliabilityDecorator(IAmazonFunctions amazonFunctions, ILog<IAmazonFunctions> log)
        {
            _inner = amazonFunctions ?? throw new ArgumentNullException(nameof(amazonFunctions));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public async Task DeleteObject(string objectKey)
        {
            try
            {
                await _inner.DeleteObject(objectKey).ConfigureAwait(false);
            }
            catch (ArgumentNullException exception)
            {
                _log.PutError($"Exception occurred: {exception.Message}");
                throw;
            }
            catch (Exception exception)
            {
                _log.PutError($"Exception occurred: {exception.Message}");
            }
        }

        public async Task<IEnumerable<S3ObjectInfo>> GetObjectsList(RemotePath prefix)
        {
            return await _inner.GetObjectsList(prefix).ConfigureAwait(false);
        }

        public async Task Purge(RemotePath prefix)
        {
            try
            {
                await _inner.Purge(prefix).ConfigureAwait(false);
            }
            catch (ArgumentNullException exception)
            {
                _log.PutError($"Exception occurred: {exception.Message}");
                throw;
            }
            catch (Amazon.S3.DeleteObjectsException exception)
            {
                _log.PutError($"Exception occurred: {exception.Message}");
                var errorResponse = exception.Response;

                foreach (var deletedObject in errorResponse.DeletedObjects)
                {
                    _log.PutError($"Deleted item  {deletedObject.Key}");
                }

                foreach (var deleteError in errorResponse.DeleteErrors)
                {
                    _log.PutError($"Error deleting item {deleteError.Key} Code - {deleteError.Code} Message - {deleteError.Message}");
                }
            }
            catch (Exception exception)
            {
                _log.PutError($"Exception occurred: {exception.Message}");
            }
        }

        public async Task UploadObjectToBucket(FileInfo fileInfo, LocalPath localPath, PartSize partSize)
        {
            try
            {
                await _inner.UploadObjectToBucket(fileInfo, localPath, partSize).ConfigureAwait(false);
            }
            catch (ArgumentNullException exception)
            {
                _log.PutError($"Exception occurred: {exception.Message}");
                throw;
            }
            catch (Exception exception)
            {
                _log.PutError($"Exception occurred: {exception.Message}");
            }
        }
    }
}
