using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;

using static System.FormattableString;

namespace S3Backup
{
    public static class S3Backup
    {
        public static async Task Main(string[] args)
        {
            var options = new Options(args);
            if (!options.IllegalArgument)
            {
                var clientInfo = GetClientInformation(options.ConfigFile);
                await Synchronize(options, GetAmazonFunctions(options.BucketName, clientInfo)).ConfigureAwait(false);
            }
            else
            {
                Log.PutOut("Synchronization can not be started in case of incorrect command line arguments");
            }
        }

        public static async Task Synchronize(Options options, IAmazonFunctions amazonFunctions)
        {
            Log.PutOut($"Synchronization started");

            var threshold = (options.RecycleAge != 0) ? DateTime.Now.Subtract(new TimeSpan(options.RecycleAge, 0, 0, 0)) : default;

            if (options.Purge && !options.DryRun)
            {
                Log.PutOut($"Purge bucket with remote path {options.RemotePath}");
                await amazonFunctions.Purge(options.RemotePath).ConfigureAwait(false);
            }

            var objects = await amazonFunctions.GetObjectsList(options.RemotePath).ConfigureAwait(false);
            Log.PutOut($"AmazonS3ObjectsList received (BucketName: {options.BucketName})");

            var filesInfo = GetFiles(options.LocalPath);
            Log.PutOut($"FileInfo dictionary received (LocalPath: {options.LocalPath})");

            foreach (var s3object in objects)
            {
                if (filesInfo.TryGetValue(s3object.Key, out var fileInfo))
                {
                    Log.PutOut($"Comparation object {s3object.Key} and file {fileInfo.Name} started");
                    filesInfo.Remove(fileInfo.Name);

                    if (EqualSize(s3object, fileInfo))
                    {
                        Log.PutOut($"Size {s3object.Key} {fileInfo.Name} matched");
                        if (options.SizeOnly)
                        {
                            continue;
                        }

                        if (EqualETag(s3object, fileInfo, options.PartSize))
                        {
                            Log.PutOut($"Hash {s3object.Key} {fileInfo.Name} matched");
                            continue;
                        }
                    }

                    if (!options.DryRun)
                    {
                        await amazonFunctions.UploadObjectToBucket(fileInfo, options.LocalPath, options.PartSize).ConfigureAwait(false);
                        Log.PutOut($"Mismatched {fileInfo.Name} uploaded");
                        continue;
                    }

                    Log.PutOut($"Mismatched {fileInfo.Name} upload skiped");
                }
                else
                {
                    Log.PutOut($"File for object key {s3object.Key} not found");
                    if (!options.DryRun && s3object.LastModified < threshold)
                    {
                        Log.PutOut($"Delete object {s3object.Key} last modified = {s3object.LastModified}");
                        await amazonFunctions.DeleteObject(s3object.Key).ConfigureAwait(false);
                        continue;
                    }

                    Log.PutOut($"Skip deleting {s3object.Key}");
                }
            }

            Log.PutOut($"Bucket does not contain {filesInfo.Count} objects");
            if (!options.DryRun)
            {
                foreach (var fileInfo in filesInfo)
                {
                    Log.PutOut($"Upload {fileInfo.Key}");
                    await amazonFunctions.UploadObjectToBucket(fileInfo.Value, options.LocalPath, options.PartSize).ConfigureAwait(false);
                    Log.PutOut("Uploaded");
                }
            }
            else
            {
                Log.PutOut("Skip upload");
            }

            Log.PutOut($"{(options.DryRun ? "DryRun" : "")} Synchronization completed");
        }

        private static ClientInformation GetClientInformation(string configFile)
            => new ConfigurationBuilder()
            .AddJsonFile(configFile)
            .Build()
            .Get<ClientInformation>() ?? new ClientInformation();

        private static IAmazonFunctions GetAmazonFunctions(string bucketName, ClientInformation clientInfo)
        {
            if (clientInfo is null)
            {
                throw new ArgumentNullException(clientInfo.GetType().ToString());
            }
            return new UseAmazon(bucketName, clientInfo);
        }

        private static Dictionary<string, FileInfo> GetFiles(string localPath)
        {
            var files = new DirectoryInfo(localPath)
                .GetFiles("*", SearchOption.AllDirectories);
            var filesInfo = new Dictionary<string, FileInfo>();
            foreach (var fileInfo in files)
            {
                filesInfo.Add(fileInfo.Name, fileInfo);
            }

            return filesInfo;
        }

        private static bool EqualETag(S3ObjectInfo s3Object, FileInfo fileInfo, int partSize)
        {
            if (string.Equals(s3Object.ETag, ComputeLocalETag(fileInfo, partSize), StringComparison.Ordinal))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static bool EqualSize(S3ObjectInfo s3Object, FileInfo fileInfo)
        {
            if (fileInfo.Length == s3Object.Size)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static string ComputeLocalETag(FileInfo file, int partSize)
        {
            var br = new BinaryReader(new FileStream(file.FullName, FileMode.Open));
            var sumIndex = 0;
            var parts = 0;
            var md5 = MD5.Create();
            var hashLength = md5.HashSize / 8;
            var n = ((file.Length / partSize) * hashLength) + ((file.Length % partSize != 0) ? hashLength : 0);
            var sum = new byte[n];
            var a = (file.Length > partSize) ? partSize : (int)file.Length;
            while (sumIndex < sum.Length)
            {
                md5.ComputeHash(br.ReadBytes(a)).CopyTo(sum, sumIndex);
                parts++;
                if (parts * partSize > file.Length)
                {
                    a = (int)file.Length % partSize;
                }

                sumIndex += hashLength;
            }

            if (parts > 1)
            {
                sum = md5.ComputeHash(sum);
            }

            var localETag = "";
            for (var i = 0; i < sum.Length; i++)
            {
                localETag = Invariant($"{localETag}{sum[i].ToString("x2")}");
            }

            localETag = $"\"{localETag}{((parts > 1) ? $"-{parts}\"" : "\"")}";

            return localETag;
        }
    }
}
