using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

using static System.FormattableString;

namespace S3Backup
{
    public class SynchronizationFunctions : ISynchronizationFunctions
    {
        private readonly Options _options;
        private readonly IAmazonFunctions _amazonFunctions;

        public SynchronizationFunctions(Options options, IAmazonFunctions amazonFunctions)
        {
            _options = options;
            _amazonFunctions = amazonFunctions;
        }

        public Dictionary<string, FileInfo> GetFiles()
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

        public async Task<bool> TryUploadMissingFiles(Dictionary<string, FileInfo> filesInfo)
        {
            if (!_options.DryRun)
            {
                foreach (var fileInfo in filesInfo)
                {
                    await _amazonFunctions.UploadObjectToBucket(fileInfo.Value, _options.LocalPath, _options.PartSize).ConfigureAwait(false);
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> TryUploadMismatchedFile(FileInfo fileInfo)
        {
            if (!_options.DryRun)
            {
                await _amazonFunctions.UploadObjectToBucket(fileInfo, _options.LocalPath, _options.PartSize).ConfigureAwait(false);
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> TryDeleteMismatchedObject(S3ObjectInfo s3Object)
        {
            var threshold = (_options.RecycleAge != 0) ? DateTime.Now.Subtract(new TimeSpan(_options.RecycleAge, 0, 0, 0)) : default;
            if (!_options.DryRun && s3Object.LastModified < threshold)
            {
                await _amazonFunctions.DeleteObject(s3Object.Key).ConfigureAwait(false);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool EqualSize(S3ObjectInfo s3Object, FileInfo fileInfo)
        {
            if (s3Object.Size == fileInfo.Length)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool EqualETag(S3ObjectInfo s3Object, FileInfo fileInfo)
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
                    localETag = Invariant($"{localETag}{sum[i]:x2}");
                }

                localETag = Invariant($"\"{localETag}{((parts > 1) ? $"-{parts}\"" : "\"")}");
            }

            return localETag;
        }
    }
}
