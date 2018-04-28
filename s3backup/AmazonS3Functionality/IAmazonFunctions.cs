﻿using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace S3Backup.AmazonS3Functionality
{
    public interface IAmazonFunctions
    {
        Task<IEnumerable<S3ObjectInfo>> GetObjectsList(RemotePath prefix);

        Task UploadObjectToBucket(FileInfo fileInfo, ObjectKeyCreator keyCreator, PartSize partSize);

        Task DeleteObject(string objectKey);

        Task Purge(RemotePath prefix);
    }
}
