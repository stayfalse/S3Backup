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
        private readonly IAmazonFunctions _amazonFunctions;

        public SynchronizationFunctions(IAmazonFunctions amazonFunctions)
        {
            _amazonFunctions = amazonFunctions;
        }

        public Dictionary<string, FileInfo> GetFiles(string localPath)
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

        public async Task<bool> TryUploadMissingFiles(Dictionary<string, FileInfo> filesInfo, bool dryRun, string localPath, int partSize)
        {
            if (!dryRun)
            {
                foreach (var fileInfo in filesInfo)
                {
                    await _amazonFunctions.UploadObjectToBucket(fileInfo.Value, localPath, partSize).ConfigureAwait(false);
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> TryUploadMismatchedFile(FileInfo fileInfo, bool dryRun, string localPath, int partSize)
        {
            if (!dryRun)
            {
                await _amazonFunctions.UploadObjectToBucket(fileInfo, localPath, partSize).ConfigureAwait(false);
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> TryDeleteMismatchedObject(S3ObjectInfo s3Object, bool dryRun, int recycleAge)
        {
            var threshold = (recycleAge != 0) ? DateTime.Now.Subtract(new TimeSpan(recycleAge, 0, 0, 0)) : default;
            if (!dryRun && s3Object.LastModified < threshold)
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
            return s3Object.Size == fileInfo.Length;
        }

        public bool EqualETag(S3ObjectInfo s3Object, FileInfo fileInfo, int partSize)
        {
            return string.Equals(s3Object.ETag, ComputeLocalETag(fileInfo, partSize), StringComparison.Ordinal);
        }

        private string ComputeLocalETag(FileInfo file, int partSize)
        {
            var localETag = "";
            using (var md5 = MD5.Create())
            {
                var br = new BinaryReader(new FileStream(file.FullName, FileMode.Open));
                var sumIndex = 0;
                var parts = 0;
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
