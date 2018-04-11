﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;

namespace S3Backup
{
    internal sealed class UseAmazon : IAmazonFunctions
    {
        private readonly Lazy<BucketName> _bucketName;
        private readonly Lazy<AmazonS3Client> _client;

        public UseAmazon(IOptionsSource optionsSource)
        {
            var options = optionsSource.AmazonOptions;
            _client = new Lazy<AmazonS3Client>(() => GetClient(options.ClientInformation));
            _bucketName = new Lazy<BucketName>(() => ConfirmBucketExistence(options.BucketName));
        }

        private BucketName BucketName => _bucketName.Value;

        private AmazonS3Client Client => _client.Value;

        public async Task<IEnumerable<S3ObjectInfo>> GetObjectsList(string prefix)
        {
            var list = new List<S3ObjectInfo>();
            foreach (var obj in await GetS3ObjectsList(prefix).ConfigureAwait(false))
            {
                var objectInfo = new S3ObjectInfo(obj.Key, obj.Size, obj.ETag, obj.LastModified);
                list.Add(objectInfo);
            }

            return list.AsReadOnly();
        }

        public async Task UploadObjectToBucket(FileInfo file, string localPath, int partSize)
        {
            var objectKey = file.FullName
                .Remove(0, localPath.Length + 1)
                .Replace('\\', '/');
            if (file.Length <= partSize)
            {
                var putObjectRequest = new PutObjectRequest
                {
                    BucketName = BucketName,
                    Key = objectKey,
                    FilePath = file.FullName,
                };
                try
                {
                    var putObjectResponse = await Client.PutObjectAsync(putObjectRequest).ConfigureAwait(false);
                }
                catch (Exception exception)
                {
                    Log.PutError($"Exception occurred: {exception.Message}");
                }
            }
            else
            {
                var multipartUploadRequest = new InitiateMultipartUploadRequest()
                {
                    BucketName = BucketName,
                    Key = objectKey,
                };

                var multipartUploadResponse = await Client.InitiateMultipartUploadAsync(multipartUploadRequest).ConfigureAwait(false);
                var a = (file.Length > partSize) ? partSize : (int)file.Length;
                try
                {
                    var list = new List<Task<UploadPartResponse>>();
                    for (var i = 0; partSize * i < file.Length; i++)
                    {
                        var upload = new UploadPartRequest()
                        {
                            BucketName = BucketName,
                            Key = objectKey,
                            UploadId = multipartUploadResponse.UploadId,
                            PartNumber = i + 1,
                            PartSize = a,
                            FilePosition = partSize * i,
                            FilePath = file.FullName,
                        };
                        if ((i + 1) * partSize > file.Length)
                        {
                            a = (int)file.Length % partSize;
                        }

                        list.Add(Client.UploadPartAsync(upload));
                    }

                    var partResponses = Task.WhenAll(list);
                    var compRequest = new CompleteMultipartUploadRequest
                    {
                        BucketName = BucketName,
                        Key = objectKey,
                        UploadId = multipartUploadResponse.UploadId,
                    };
                    compRequest.AddPartETags(await partResponses.ConfigureAwait(false));
                    var completeUploadResponse = await Client.CompleteMultipartUploadAsync(compRequest).ConfigureAwait(false);
                }
                catch (Exception exception)
                {
                    Log.PutError($"Exception occurred: {exception.Message}");
                    var abortMPURequest = new AbortMultipartUploadRequest
                    {
                        BucketName = BucketName,
                        Key = objectKey,
                        UploadId = multipartUploadResponse.UploadId,
                    };
                    await Client.AbortMultipartUploadAsync(abortMPURequest).ConfigureAwait(false);
                }
            }
        }

        public async Task DeleteObject(string key)
        {
            var deleteRequest = new DeleteObjectRequest
            {
                BucketName = BucketName,
                Key = key,
            };
            var deleteResponse = await Client.DeleteObjectAsync(deleteRequest).ConfigureAwait(false);
            Log.PutOut($"{key} deleted from bucket");
        }

        public async Task Purge(string prefix)
        {
            var deleteTasks = new List<Task>();
            foreach (var obj in await GetS3ObjectsList(prefix).ConfigureAwait(false))
            {
                deleteTasks.Add(DeleteObject(obj.Key));
            }

            await Task.WhenAll(deleteTasks).ConfigureAwait(false);
        }

        private AmazonS3Client GetClient(ClientInformation config)
        {
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
                Log.PutError($"Exception occurred: {exception.Message}");
                return null;
            }
        }

        private BucketName ConfirmBucketExistence(BucketName bucketName)
        {
            if (!AmazonS3Util.DoesS3BucketExistAsync(Client, bucketName).ConfigureAwait(false).GetAwaiter().GetResult())
            {
                PutBucketToAmazon(bucketName).ConfigureAwait(false).GetAwaiter().GetResult();
            }

            return bucketName;
        }


        private async Task PutBucketToAmazon(string bucket)
        {
            var putRequest = new PutBucketRequest
            {
                BucketName = bucket,
            };
            await Client.PutBucketAsync(putRequest).ConfigureAwait(false);
        }

        private async Task<List<S3Object>> GetS3ObjectsList(string prefix)
        {
            var request = new ListObjectsRequest
            {
                BucketName = BucketName,
                Prefix = prefix,
            };

            var objects = (await Client.ListObjectsAsync(request).ConfigureAwait(false)).S3Objects;
            return objects;
        }

        private async Task DeleteObjects(List<S3Object> objects) // does not work, "Access denied" exception
        {
            try
            {
                var deleteRequest = new DeleteObjectsRequest { BucketName = BucketName };
                foreach (var obj in objects)
                {
                    deleteRequest.AddKey(obj.Key);
                    Log.PutOut($"{obj.Key} added to keyversion List for delete objects");
                }

                var deleteResponse = await Client.DeleteObjectsAsync(deleteRequest).ConfigureAwait(false);
                Log.PutOut($"{objects.Capacity} objects deleted");
            }
            catch (DeleteObjectsException exception)
            {
                Log.PutError($"Exception occurred: {exception.Message}");
                var errorResponse = exception.Response;

                foreach (var deletedObject in errorResponse.DeletedObjects)
                {
                    Log.PutError($"Deleted item  {deletedObject.Key}");
                }

                foreach (var deleteError in errorResponse.DeleteErrors)
                {
                    Log.PutError($"Error deleting item {deleteError.Key} Code - {deleteError.Code} Message - {deleteError.Message}");
                }
            }
        }
    }
}
