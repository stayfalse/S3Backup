﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;

namespace S3Backup.AmazonS3Functionality
{
    internal sealed class UseAmazon : IAmazonFunctions
    {
        private readonly BucketName _bucketName;
        private readonly ParallelParts _parallelParts;
        private readonly Lazy<AmazonS3Client> _client;
        private readonly Lazy<Task> _initializer;

        public UseAmazon(IOptionsSource optionsSource)
        {
            if (optionsSource is null)
            {
                throw new ArgumentNullException(nameof(optionsSource));
            }

            var options = optionsSource.AmazonOptions;
            _client = new Lazy<AmazonS3Client>(GetClient(options.ClientInformation));
            _bucketName = options.BucketName;
            _parallelParts = options.ParallelParts;
            _initializer = new Lazy<Task>(DoInitialize());
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

                var putObjectResponse = await Client.PutObjectAsync(putObjectRequest).ConfigureAwait(false);
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

            var s3Config = new AmazonS3Config
            {
                ServiceURL = config.ServiceUrl.AbsoluteUri,
            };
            return new AmazonS3Client(config.AccessKey, config.SecretKey, s3Config);
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

            if (_parallelParts is null)
            {
                throw new ArgumentNullException(nameof(_parallelParts));
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
                var partResponses = new List<UploadPartResponse>();
                var parallelOptions = new ParallelOptions
                {
                    MaxDegreeOfParallelism = Environment.ProcessorCount,
                };

                Parallel.For(0, (fileInfo.Length / partSize) + 1, parallelOptions, async (i) =>
                {
                    Console.WriteLine($"try to upload part {i}");
                    using (var stream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, a, true)
                    {
                        Position = partSize * i,
                    })
                    {
                        var upload = new UploadPartRequest()
                        {
                            BucketName = _bucketName,
                            Key = objectKey,
                            UploadId = multipartUploadResponse.UploadId,
                            PartNumber = (int)i + 1,
                            PartSize = a,
                            InputStream = stream,
                        };
                        if ((i + 1) * partSize > fileInfo.Length)
                        {
                            a = (int)fileInfo.Length % partSize;
                        }

                        partResponses.Add(await Client.UploadPartAsync(upload).ConfigureAwait(false));
                    }

                    Console.WriteLine($"uploaded part {i}");
                });

                var completeRequest = new CompleteMultipartUploadRequest
                {
                    BucketName = _bucketName,
                    Key = objectKey,
                    UploadId = multipartUploadResponse.UploadId,
                };
                completeRequest.AddPartETags(partResponses);
                var completeUploadResponse = await Client.CompleteMultipartUploadAsync(completeRequest).ConfigureAwait(false);
            }
            catch
            {
                var abortMultipartUploadRequest = new AbortMultipartUploadRequest
                {
                    BucketName = _bucketName,
                    Key = objectKey,
                    UploadId = multipartUploadResponse.UploadId,
                };
                await Client.AbortMultipartUploadAsync(abortMultipartUploadRequest).ConfigureAwait(false);
                throw;
            }
        }

        private async Task DeleteObjects(List<S3Object> objects) // does not work, "Access denied" exception
        {
            if (objects is null)
            {
                throw new ArgumentNullException(nameof(objects));
            }

            await Initialize().ConfigureAwait(false);
            var deleteRequest = new DeleteObjectsRequest { BucketName = _bucketName };
            foreach (var obj in objects)
            {
                deleteRequest.AddKey(obj.Key);
            }

            var deleteResponse = await Client.DeleteObjectsAsync(deleteRequest).ConfigureAwait(false);
        }
    }
}
