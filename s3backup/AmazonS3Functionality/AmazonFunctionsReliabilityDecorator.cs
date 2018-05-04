using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

using Amazon.S3;

using S3Backup.Logging;

namespace S3Backup.AmazonS3Functionality
{
    public class AmazonFunctionsReliabilityDecorator : IAmazonFunctions
    {
        private const int IntervalThreshold = 16384;
        private const int StartInterval = 128;
        private const int Timeout = 120000;
        private readonly IAmazonFunctions _inner;
        private readonly ILog<IAmazonFunctions> _log;

        public AmazonFunctionsReliabilityDecorator(IAmazonFunctions amazonFunctions, ILog<IAmazonFunctions> log)
        {
            _inner = amazonFunctions ?? throw new ArgumentNullException(nameof(amazonFunctions));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public async Task DeleteObject(string objectKey)
        {
            await CommonExceptionHandler(_inner.DeleteObject(objectKey)).ConfigureAwait(false);
        }

        public async Task<IEnumerable<S3ObjectInfo>> GetObjectsList(RemotePath prefix)
        {
            return await _inner.GetObjectsList(prefix).ConfigureAwait(false);
        }

        public async Task Purge(RemotePath prefix)
        {
            try
            {
                await CommonExceptionHandler(_inner.Purge(prefix)).ConfigureAwait(false);
            }
            catch (DeleteObjectsException exception)
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
        }

        public async Task UploadObjectToBucket(FileInfo fileInfo, ObjectKeyCreator keyCreator, PartSize partSize)
        {
            await CommonExceptionHandler(_inner.UploadObjectToBucket(fileInfo, keyCreator, partSize)).ConfigureAwait(false);
        }

        private async Task CommonExceptionHandler(Task task)
        {
            try
            {
                await task.ConfigureAwait(false);
            }
            catch (AmazonS3Exception exception)
            when (string.Equals(exception.ErrorCode, "InternalError", StringComparison.Ordinal))
            {
                _log.PutError($"Exception occurred: {exception.Message}");
                await Retry(task, exception).ConfigureAwait(false);
            }
            catch (DeleteObjectsException)
            {
                throw;
            }
            catch (AmazonS3Exception exception)
            {
                _log.PutError($"Exception occurred: {exception.Message}");
            }
        }

        private async Task Retry(Task task, Exception innerException)
        {
            var retryInterval = StartInterval;
            var watch = new Stopwatch();
            watch.Start();
            while (retryInterval <= IntervalThreshold)
            {
                await Task.Delay(retryInterval).ConfigureAwait(false);
                try
                {
                    await task.ConfigureAwait(false);
                    break;
                }
                catch (Exception exception)
                when (string.Equals(exception.Message, innerException.Message, StringComparison.Ordinal))
                {
                    retryInterval = (retryInterval == IntervalThreshold) ? retryInterval : retryInterval * 2;
                }

                if (watch.ElapsedMilliseconds + retryInterval >= Timeout)
                {
                    watch.Stop();
                    break;
                }
            }
        }
    }
}
