using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace S3Backup
{
    public interface ISynchronizationFunctions
    {
        Dictionary<string, FileInfo> GetFiles(LocalPath localPath);

        Task<bool> TryUploadMissingFiles(Dictionary<string, FileInfo> filesInfo, bool dryRun, LocalPath localPath, PartSize partSize);

        Task<bool> TryUploadMismatchedFile(FileInfo fileInfo, bool dryRun, LocalPath localPath, PartSize partSize);

        Task<bool> TryDeleteMismatchedObject(S3ObjectInfo s3Object, bool dryRun, ThresholdDate threshold);

        bool EqualSize(S3ObjectInfo s3Object, FileInfo fileInfo);

        bool EqualETag(S3ObjectInfo s3Object, FileInfo fileInfo, PartSize partSize);
    }
}
