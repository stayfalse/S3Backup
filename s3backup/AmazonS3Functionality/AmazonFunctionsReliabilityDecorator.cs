using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
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
            return await CommonExceptionHandler(_inner.GetObjectsList(prefix)).ConfigureAwait(false);
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
            var retryInterval = StartInterval;
            var watch = Stopwatch.StartNew();
            while (watch.ElapsedMilliseconds + retryInterval < Timeout)
            {
                Exception repeatableException = null;
                try
                {
                    await task.ConfigureAwait(false);
                    break;
                }
                catch (WebException exception)
                when ((exception.Status == WebExceptionStatus.ConnectFailure) || (exception.Status == WebExceptionStatus.SendFailure))
                {
                    repeatableException = exception;
                }
                catch (WebException exception)
                {
                    _log.PutError($"Exception occurred: {exception.Message}");
                }
                catch (AmazonS3Exception exception)
                when (exception.ErrorCode == "InternalError")
                {
                    repeatableException = exception;
                }
                catch (DeleteObjectsException)
                {
                    throw;
                }
                catch (AmazonS3Exception exception)
                {
                    _log.PutError($"Exception occurred: {exception.Message}");
                }

                if (repeatableException != null)
                {
                    _log.PutError($"Exception occurred: {repeatableException.Message}");
                    await Task.Delay(retryInterval).ConfigureAwait(false);
                    retryInterval = (retryInterval == IntervalThreshold) ? retryInterval : retryInterval * 2;
                }
                else
                {
                    break;
                }
            }
        }

        private async Task<IEnumerable<S3ObjectInfo>> CommonExceptionHandler(Task<IEnumerable<S3ObjectInfo>> task)
        {
            var retryInterval = StartInterval;
            var watch = Stopwatch.StartNew();
            while (watch.ElapsedMilliseconds + retryInterval < Timeout)
            {
                Exception repeatableException = null;
                try
                {
                    return await task.ConfigureAwait(false);
                }
                catch (WebException exception)
                when ((exception.Status == WebExceptionStatus.ConnectFailure)
                || (exception.Status == WebExceptionStatus.SendFailure)
                || (exception.Status == WebExceptionStatus.ReceiveFailure))
                {
                    repeatableException = exception;
                }
                catch (AmazonS3Exception exception)
                when (exception.ErrorCode == "InternalError")
                {
                    repeatableException = exception;
                }

                _log.PutError($"Exception occurred: {repeatableException.Message}");
                await Task.Delay(retryInterval).ConfigureAwait(false);
                retryInterval = (retryInterval == IntervalThreshold) ? retryInterval : retryInterval * 2;
            }

            throw new TimeoutException();
        }
    }
}
