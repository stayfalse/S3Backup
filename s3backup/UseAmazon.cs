using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;

namespace S3Backup
{
    public class UseAmazon
    {
        private AmazonS3Client Client;
        private string Bucket { get; set; }

        public UseAmazon(string bucketName, ClientInformation config)
        {
            try
            {
                var s3Config = new AmazonS3Config
                {
                    ServiceURL = config.ServiceUrl
                };
                Client = new AmazonS3Client(config.AccessKey, config.SecretKey, s3Config);
            }
            catch (Exception exception)
            {
                Log.PutError($"Exception occurred: {exception.Message}");
            }
            Bucket = bucketName;
            CheckBucketExistance().GetAwaiter().GetResult();
        }

        private async Task CheckBucketExistance()
        {
            if (!await AmazonS3Util.DoesS3BucketExistAsync(Client, Bucket))
            {
                var putRequest = new PutBucketRequest
                {
                    BucketName = Bucket
                };
                await Client.PutBucketAsync(putRequest).ConfigureAwait(false);
            }
        }

        public async Task<List<S3Object>> GetS3ObjectsList(string prefix = "")
        {
            var request = new ListObjectsRequest
            {
                BucketName = Bucket,
                Prefix = prefix
            };

            var objects = (await Client.ListObjectsAsync(request).ConfigureAwait(false)).S3Objects;
            return objects;
        }

        public async Task UploadObjectToBucket(FileInfo file, string localPath, int partSize)
        {
            var objectKey = file.FullName.Remove(0, localPath.Length + 1).Replace("\\", "/");
            if (file.Length <= partSize)
            {
                PutObjectRequest putObjectRequest = new PutObjectRequest
                {
                    BucketName = Bucket,
                    Key = objectKey,
                    FilePath = file.FullName
                };
                try
                {
                    PutObjectResponse putObjectResponse = await Client.PutObjectAsync(putObjectRequest).ConfigureAwait(false);
                }
                catch (Exception exception)
                {
                    Log.PutError($"Exception occurred: {exception.Message}");
                }
            }
            else
            {
                InitiateMultipartUploadRequest multipartUploadRequest = new InitiateMultipartUploadRequest()
                {
                    BucketName = Bucket,
                    Key = objectKey
                };

                var multipartUploadResponse = await Client.InitiateMultipartUploadAsync(multipartUploadRequest).ConfigureAwait(false);
                int a = (file.Length > partSize) ? partSize : (int)file.Length;
                try
                { 
                    List<Task<UploadPartResponse>> list = new List<Task<UploadPartResponse>>();
                    for (int i = 0; partSize * i < file.Length; i++)
                    {
                        UploadPartRequest upload = new UploadPartRequest()
                        {
                            BucketName = Bucket,
                            Key = objectKey,
                            UploadId = multipartUploadResponse.UploadId,
                            PartNumber = i + 1,
                            PartSize = a,
                            FilePosition = partSize * i,
                            FilePath = file.FullName,
                        };
                        if ((i + 1) * partSize > file.Length)
                        { a = (int)file.Length % partSize; }
                        list.Add(Client.UploadPartAsync(upload));
                    }
                    var partResponses = Task.WhenAll(list);
                    CompleteMultipartUploadRequest compRequest = new CompleteMultipartUploadRequest
                    {
                        BucketName = Bucket,
                        Key = objectKey,
                        UploadId = multipartUploadResponse.UploadId,
                    };
                    compRequest.AddPartETags(await partResponses.ConfigureAwait(false));
                    var completeUploadResponse = await Client.CompleteMultipartUploadAsync(compRequest).ConfigureAwait(false);
                }
                catch (Exception exception)
                {
                    Log.PutError($"Exception occurred: {exception.Message}");
                    AbortMultipartUploadRequest abortMPURequest = new AbortMultipartUploadRequest
                    {
                        BucketName = Bucket,
                        Key = objectKey,
                        UploadId = multipartUploadResponse.UploadId
                    };
                    await Client.AbortMultipartUploadAsync(abortMPURequest).ConfigureAwait(false);
                }
            }
        }

        public async Task DeleteObject(string objectKey)
        {
            var deleteRequest = new DeleteObjectRequest
            {
                BucketName = Bucket,
                Key = objectKey
            };
            var deleteResponse = await Client.DeleteObjectAsync(deleteRequest).ConfigureAwait(false);
            Log.PutOut($"{objectKey} deleted from bucket");
        }

        private async Task DeleteObjects(List<S3Object> objects) // does not work, "Acces denied" exeption
        {
            
            CancellationTokenSource sourse = new CancellationTokenSource();
            try
            {
                var deleteRequest = new DeleteObjectsRequest { BucketName = this.Bucket };
                foreach (var obj in objects)
                {
                    deleteRequest.AddKey(obj.Key);
                    Log.PutOut($"{obj.Key} added to keyversion List for delete objects");
                }
                var deleteResponse = await Client.DeleteObjectsAsync(deleteRequest, sourse.Token).ConfigureAwait(false);
                Log.PutOut($"{objects.Capacity} objects deleted");
            }
            catch (DeleteObjectsException doe)
            {
                Log.PutError("Exception occurred: "+ doe.Message);
                DeleteObjectsResponse errorResponse = doe.Response;

                foreach (DeletedObject deletedObject in errorResponse.DeletedObjects)
                {
                    Log.PutError(" Deleted item " + deletedObject.Key);
                }
                foreach (DeleteError deleteError in errorResponse.DeleteErrors)
                {
                    Log.PutError(" Error deleting item " + deleteError.Key + " Code - " + deleteError.Code + " Message - " + deleteError.Message);
                }
            }
        }

        public async Task Purge(string prefix)
        {
            var deleteTasks = new List<Task>();
           foreach (var obj in await GetS3ObjectsList(prefix).ConfigureAwait(false))
            {
                deleteTasks.Add(DeleteObject(obj.Key));
            }
           await Task.WhenAll(deleteTasks);
        }
    }
}
