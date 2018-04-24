using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using S3Backup.Logging;

namespace S3Backup.AmazonS3Functionality
{
    public class AmazonFunctionsLoggingDecorator : IAmazonFunctions
    {
        private readonly IAmazonFunctions _inner;
        private readonly ILog<IAmazonFunctions> _log;

        public AmazonFunctionsLoggingDecorator(IAmazonFunctions amazonFunctionsDryRun, ILog<IAmazonFunctions> log)
        {
            _inner = amazonFunctionsDryRun ?? throw new ArgumentNullException(nameof(amazonFunctionsDryRun));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public async Task<IEnumerable<S3ObjectInfo>> GetObjectsList(RemotePath prefix)
        {
            if (prefix is null)
            {
                throw new ArgumentNullException(nameof(prefix));
            }

            var objectsList = _inner.GetObjectsList(prefix);
            _log.PutOut($"S3Objects list received. (RemotePath: {prefix})");
            return await objectsList.ConfigureAwait(false);
        }

        public async Task UploadObjectToBucket(FileInfo fileInfo, LocalPath localPath, PartSize partSize)
        {
            if (fileInfo is null)
            {
                throw new ArgumentNullException(nameof(fileInfo));
            }

            _log.PutOut($"Upload {fileInfo.Name} to bucket started.");
            try
            {
                await _inner.UploadObjectToBucket(fileInfo, localPath, partSize).ConfigureAwait(false);
                _log.PutOut($"Uploaded");
            }
            catch (ArgumentNullException)
            {
                throw;
            }
            catch (Exception exception)
            {
                _log.PutError($"Exception occurred: {exception.Message}");
            }
        }

        public async Task DeleteObject(string objectKey)
        {
            _log.PutOut($"Delete object {objectKey}.");
            try
            {
                await _inner.DeleteObject(objectKey).ConfigureAwait(false);
                _log.PutOut($"Deleted.");
            }
            catch (ArgumentNullException)
            {
                throw;
            }
            catch (Exception exception)
            {
                _log.PutError($"Exception occurred: {exception.Message}");
            }
        }

        public async Task Purge(RemotePath prefix)
        {
            if (prefix is null)
            {
                throw new ArgumentNullException(nameof(prefix));
            }

            _log.PutOut($"Purge bucket with remote path {prefix}");
            try
            {
                await _inner.Purge(prefix).ConfigureAwait(false);
                _log.PutOut($"Purge completed.");
            }
            catch (ArgumentNullException)
            {
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
    }
}
