using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;

using S3Backup.Logging;

namespace S3Backup.AmazonS3Functionality
{
    internal sealed class UseAmazon : IAmazonFunctions
    {
        private readonly BucketName _bucketName;
        private readonly Lazy<AmazonS3Client> _client;
        private readonly Lazy<Task> _initializer;
        private readonly ILog<IAmazonFunctions> _log;

        public UseAmazon(IOptionsSource optionsSource, ILog<IAmazonFunctions> log)
        {
            if (optionsSource is null)
            {
                throw new ArgumentNullException(nameof(optionsSource));
            }

            var options = optionsSource.AmazonOptions;
            _client = new Lazy<AmazonS3Client>(GetClient(options.ClientInformation));
            _bucketName = options.BucketName;
            _initializer = new Lazy<Task>(DoInitialize());
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        private AmazonS3Client Client => _client.Value;

        public async Task<IEnumerable<S3ObjectInfo>> GetObjectsList(RemotePath prefix)
        {
            if (prefix is null)
            {
                throw new ArgumentNullException(nameof(prefix));
            }

            var list = new List<S3ObjectInfo>();
            foreach (var obj in await GetS3ObjectsList(prefix).ConfigureAwait(false))
            {
                var objectInfo = new S3ObjectInfo(obj.Key, obj.Size, obj.ETag, obj.LastModified);
                list.Add(objectInfo);
            }

            return list.AsReadOnly();
        }

        public async Task UploadObjectToBucket(FileInfo fileInfo, LocalPath localPath, PartSize partSize)
        {
            if (fileInfo is null)
            {
                throw new ArgumentNullException(nameof(fileInfo));
            }

            if (localPath is null)
            {
                throw new ArgumentNullException(nameof(localPath));
            }

            if (partSize is null)
            {
                throw new ArgumentNullException(nameof(partSize));
            }

            await Initialize().ConfigureAwait(false);

            var objectKey = fileInfo.FullName
                .Remove(0, localPath.Length + 1)
                .Replace('\\', '/');
            if (fileInfo.Length <= partSize)
            {
                var putObjectRequest = new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = objectKey,
                    FilePath = fileInfo.FullName,
                };
                try
                {
                    var putObjectResponse = await Client.PutObjectAsync(putObjectRequest).ConfigureAwait(false);
                }
                catch (Exception exception)
                {
                    _log.PutError($"Exception occurred: {exception.Message}");
                }
            }
            else
            {
                await MultipartUploadObject(fileInfo, objectKey, partSize).ConfigureAwait(false);
            }
        }

        public async Task DeleteObject(string objectKey)
        {
            if (string.IsNullOrEmpty(objectKey))
            {
                throw new ArgumentException($"{nameof(objectKey)} is null or empty.");
            }

            await Initialize().ConfigureAwait(false);
            var deleteRequest = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = objectKey,
            };
            var deleteResponse = await Client.DeleteObjectAsync(deleteRequest).ConfigureAwait(false);
        }

        public async Task Purge(RemotePath prefix)
        {
            if (prefix is null)
            {
                throw new ArgumentNullException(nameof(prefix));
            }

            var deleteTasks = new List<Task>();
            foreach (var obj in await GetS3ObjectsList(prefix).ConfigureAwait(false))
            {
                deleteTasks.Add(DeleteObject(obj.Key));
            }

            await Task.WhenAll(deleteTasks).ConfigureAwait(false);
        }

        private AmazonS3Client GetClient(ClientInformation config)
        {
            if (config is null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            try
            {
                var s3Config = new AmazonS3Config
                {
                    ServiceURL = config.ServiceUrl,
                };
                return new AmazonS3Client(config.AccessKey, config.SecretKey, s3Config);
            }
            catch (Exception exception)
            {
                _log.PutError($"Exception occurred: {exception.Message}");
                return null;
            }
        }

        private async Task Initialize() => await _initializer.Value.ConfigureAwait(false);

        private async Task DoInitialize()
        {
            if (!await AmazonS3Util.DoesS3BucketExistAsync(Client, _bucketName).ConfigureAwait(false))
            {
                var putRequest = new PutBucketRequest
                {
                    BucketName = _bucketName,
                };
                await Client.PutBucketAsync(putRequest).ConfigureAwait(false);
            }
        }

        private async Task<List<S3Object>> GetS3ObjectsList(RemotePath prefix)
        {
            if (prefix is null)
            {
                throw new ArgumentNullException(nameof(prefix));
            }

            await Initialize().ConfigureAwait(false);
            var request = new ListObjectsRequest
            {
                BucketName = _bucketName,
                Prefix = prefix,
            };

            var objects = (await Client.ListObjectsAsync(request).ConfigureAwait(false)).S3Objects;
            return objects;
        }

        private async Task MultipartUploadObject(FileInfo fileInfo, string objectKey, PartSize partSize)
        {
            if (fileInfo is null)
            {
                throw new ArgumentNullException(nameof(fileInfo));
            }

            if (partSize is null)
            {
                throw new ArgumentNullException(nameof(partSize));
            }

            var multipartUploadRequest = new InitiateMultipartUploadRequest()
            {
                BucketName = _bucketName,
                Key = objectKey,
            };

            await Initialize().ConfigureAwait(false);

            var multipartUploadResponse = await Client.InitiateMultipartUploadAsync(multipartUploadRequest).ConfigureAwait(false);
            var a = (fileInfo.Length > partSize) ? partSize : (int)fileInfo.Length;
            try
            {
                var list = new List<Task<UploadPartResponse>>();
                for (var i = 0; partSize * i < fileInfo.Length; i++)
                {
                    var upload = new UploadPartRequest()
                    {
                        BucketName = _bucketName,
                        Key = objectKey,
                        UploadId = multipartUploadResponse.UploadId,
                        PartNumber = i + 1,
                        PartSize = a,
                        FilePosition = partSize * i,
                        FilePath = fileInfo.FullName,
                    };
                    if ((i + 1) * partSize > fileInfo.Length)
                    {
                        a = (int)fileInfo.Length % partSize;
                    }

                    list.Add(Client.UploadPartAsync(upload));
                }

                var partResponses = Task.WhenAll(list);
                var compRequest = new CompleteMultipartUploadRequest
                {
                    BucketName = _bucketName,
                    Key = objectKey,
                    UploadId = multipartUploadResponse.UploadId,
                };
                compRequest.AddPartETags(await partResponses.ConfigureAwait(false));
                var completeUploadResponse = await Client.CompleteMultipartUploadAsync(compRequest).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                _log.PutError($"Exception occurred: {exception.Message}");
                var abortMPURequest = new AbortMultipartUploadRequest
                {
                    BucketName = _bucketName,
                    Key = objectKey,
                    UploadId = multipartUploadResponse.UploadId,
                };
                await Client.AbortMultipartUploadAsync(abortMPURequest).ConfigureAwait(false);
            }
        }

        private async Task DeleteObjects(List<S3Object> objects) // does not work, "Access denied" exception
        {
            if (objects is null)
            {
                throw new ArgumentNullException(nameof(objects));
            }

            await Initialize().ConfigureAwait(false);
            try
            {
                var deleteRequest = new DeleteObjectsRequest { BucketName = _bucketName };
                foreach (var obj in objects)
                {
                    deleteRequest.AddKey(obj.Key);
                    _log.PutOut($"{obj.Key} added to keyversion List for delete objects");
                }

                var deleteResponse = await Client.DeleteObjectsAsync(deleteRequest).ConfigureAwait(false);
                _log.PutOut($"{objects.Capacity} objects deleted");
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
    }
}
