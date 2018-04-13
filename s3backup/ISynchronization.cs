using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace S3Backup
{
    public interface ISynchronization
    {
        Task Synchronize();

        Task<IEnumerable<FileInfo>> CompareLocalFilesAndS3Objects(IEnumerable<S3ObjectInfo> objects);

        bool FileEqualsObject(FileInfo fileInfo, S3ObjectInfo s3Object);
    }
}
