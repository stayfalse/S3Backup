﻿using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace S3Backup
{
    public interface IAmazonFunctions
    {
        Task<IEnumerable<S3ObjectInfo>> GetObjectsList(RemotePath prefix);

        Task UploadObjectToBucket(FileInfo file, LocalPath localPath, PartSize partSize);

        Task DeleteObject(string key);

        Task Purge(RemotePath prefix);
    }
}
