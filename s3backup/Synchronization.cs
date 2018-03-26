using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

using static System.FormattableString;

namespace S3Backup
{
    public class Synchronization
    {
        private readonly Options _options; // should set only usable options
        private readonly IAmazonFunctions _amazonFunctions;

        public Synchronization(Options options, IAmazonFunctions amazonFunctions)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (amazonFunctions is null)
            {
                throw new ArgumentNullException(nameof(amazonFunctions));
            }

            _options = options;
            _amazonFunctions = amazonFunctions;
        }

        public async Task Synchronize()
        {
            Log.PutOut($"Synchronization started");

            if (_options.Purge && !_options.DryRun)
            {
                Log.PutOut($"Purge bucket with remote path {_options.RemotePath}");
                await _amazonFunctions.Purge(_options.RemotePath).ConfigureAwait(false);
            }

            var objects = await _amazonFunctions.GetObjectsList(_options.RemotePath).ConfigureAwait(false);
            Log.PutOut($"AmazonS3ObjectsList received (RemotePath: {_options.RemotePath})");
            var filesInfo = GetFiles();
            Log.PutOut($"FileInfo dictionary received (LocalPath: {_options.LocalPath})");
            filesInfo = await CompareFilesAndObjectsLists(filesInfo, objects).ConfigureAwait(false);

            Log.PutOut($"Bucket does not contain {filesInfo.Count} objects");
            if (!_options.DryRun)
            {
                foreach (var fileInfo in filesInfo)
                {
                    Log.PutOut($"Upload {fileInfo.Key}");
                    await _amazonFunctions.UploadObjectToBucket(fileInfo.Value, _options.LocalPath, _options.PartSize).ConfigureAwait(false);
                    Log.PutOut("Uploaded");
                }
            }
            else
            {
                Log.PutOut("Skip upload");
            }

            Log.PutOut($"{(_options.DryRun ? "DryRun" : "")} Synchronization completed");
        }

        private Dictionary<string, FileInfo> GetFiles()
        {
            var files = new DirectoryInfo(_options.LocalPath)
                .GetFiles("*", SearchOption.AllDirectories);
            var filesInfo = new Dictionary<string, FileInfo>();
            foreach (var fileInfo in files)
            {
                filesInfo.Add(fileInfo.Name, fileInfo);
            }

            return filesInfo;
        }

        private async Task<Dictionary<string, FileInfo>> CompareFilesAndObjectsLists(Dictionary<string, FileInfo> filesInfo, IEnumerable<S3ObjectInfo> objects)
        {
            var threshold = (_options.RecycleAge != 0) ? DateTime.Now.Subtract(new TimeSpan(_options.RecycleAge, 0, 0, 0)) : default;
            foreach (var s3object in objects)
            {
                if (filesInfo.TryGetValue(s3object.Key, out var fileInfo))
                {
                    Log.PutOut($"Comparation object {s3object.Key} and file {fileInfo.Name} started");
                    filesInfo.Remove(fileInfo.Name);

                    if (fileInfo.Length == s3object.Size)
                    {
                        Log.PutOut($"Size {s3object.Key} {fileInfo.Name} matched");
                        if (_options.SizeOnly)
                        {
                            continue;
                        }

                        if (EqualETag(s3object, fileInfo))
                        {
                            Log.PutOut($"Hash {s3object.Key} {fileInfo.Name} matched");
                            continue;
                        }
                    }

                    if (!_options.DryRun)
                    {
                        await _amazonFunctions.UploadObjectToBucket(fileInfo, _options.LocalPath, _options.PartSize).ConfigureAwait(false);
                        Log.PutOut($"Mismatched {fileInfo.Name} uploaded");
                        continue;
                    }

                    Log.PutOut($"Mismatched {fileInfo.Name} upload skiped");
                }
                else
                {
                    Log.PutOut($"File for object key {s3object.Key} not found");
                    if (!_options.DryRun && s3object.LastModified < threshold)
                    {
                        Log.PutOut($"Delete object {s3object.Key} last modified = {s3object.LastModified}");
                        await _amazonFunctions.DeleteObject(s3object.Key).ConfigureAwait(false);
                        continue;
                    }

                    Log.PutOut($"Skip deleting {s3object.Key}");
                }
            }

            return filesInfo;
        }

        private bool EqualETag(S3ObjectInfo s3Object, FileInfo fileInfo)
        {
            if (string.Equals(s3Object.ETag, ComputeLocalETag(fileInfo), StringComparison.Ordinal))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private string ComputeLocalETag(FileInfo file)
        {
            var localETag = "";
            using (var md5 = MD5.Create())
            {
                var br = new BinaryReader(new FileStream(file.FullName, FileMode.Open));
                var sumIndex = 0;
                var parts = 0;
                var hashLength = md5.HashSize / 8;
                var n = ((file.Length / _options.PartSize) * hashLength) + ((file.Length % _options.PartSize != 0) ? hashLength : 0);
                var sum = new byte[n];
                var a = (file.Length > _options.PartSize) ? _options.PartSize : (int)file.Length;
                while (sumIndex < sum.Length)
                {
                    md5.ComputeHash(br.ReadBytes(a)).CopyTo(sum, sumIndex);
                    parts++;
                    if (parts * _options.PartSize > file.Length)
                    {
                        a = (int)file.Length % _options.PartSize;
                    }

                    sumIndex += hashLength;
                }

                if (parts > 1)
                {
                    sum = md5.ComputeHash(sum);
                }

                for (var i = 0; i < sum.Length; i++)
                {
                    localETag = Invariant($"{localETag}{sum[i].ToString("x2")}");
                }

                localETag = $"\"{localETag}{((parts > 1) ? $"-{parts}\"" : "\"")}";
            }

            return localETag;
        }
    }
}
