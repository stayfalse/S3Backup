using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

using static System.FormattableString;

namespace S3Backup
{
    public class SynchronizationFunctions : ISynchronizationFunctions
    {
        public Dictionary<string, FileInfo> GetFiles(LocalPath localPath)
        {
            if (localPath is null)
            {
                throw new ArgumentNullException(nameof(localPath));
            }

            var files = new DirectoryInfo(localPath)
                .GetFiles("*", SearchOption.AllDirectories);
            var filesInfo = new Dictionary<string, FileInfo>();
            foreach (var fileInfo in files)
            {
                filesInfo.Add(fileInfo.Name, fileInfo);
            }

            return filesInfo;
        }

        public bool EqualSize(S3ObjectInfo s3Object, FileInfo fileInfo)
        {
            if (s3Object is null)
            {
                throw new ArgumentNullException(nameof(s3Object));
            }

            if (fileInfo is null)
            {
                throw new ArgumentNullException(nameof(fileInfo));
            }

            return s3Object.Size == fileInfo.Length;
        }

        public bool EqualETag(S3ObjectInfo s3Object, FileInfo fileInfo, PartSize partSize)
        {
            if (s3Object is null)
            {
                throw new ArgumentNullException(nameof(s3Object));
            }

            return string.Equals(s3Object.ETag, ComputeLocalETag(fileInfo, partSize), StringComparison.Ordinal);
        }

        private static string ComputeLocalETag(FileInfo fileInfo, PartSize partSize)
        {
            if (fileInfo is null)
            {
                throw new ArgumentNullException(nameof(fileInfo));
            }

            if (partSize is null)
            {
                throw new ArgumentNullException(nameof(partSize));
            }

            var localETag = "";
            using (var md5 = MD5.Create())
            {
                var br = new BinaryReader(new FileStream(fileInfo.FullName, FileMode.Open));
                var sumIndex = 0;
                var parts = 0;
                var hashLength = md5.HashSize / 8;
                var n = ((fileInfo.Length / partSize) * hashLength) + ((fileInfo.Length % partSize != 0) ? hashLength : 0);
                var sum = new byte[n];
                var a = (fileInfo.Length > partSize) ? partSize : (int)fileInfo.Length;
                while (sumIndex < sum.Length)
                {
                    md5.ComputeHash(br.ReadBytes(a)).CopyTo(sum, sumIndex);
                    parts++;
                    if (parts * partSize > fileInfo.Length)
                    {
                        a = (int)fileInfo.Length % partSize;
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
