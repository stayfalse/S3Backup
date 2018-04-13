using System.Collections.Generic;
using System.IO;

namespace S3Backup
{
    public class SynchronizationFunctionsLoggingDecorator : ISynchronizationFunctions
    {
        private readonly ISynchronizationFunctions _inner;

        public SynchronizationFunctionsLoggingDecorator(ISynchronizationFunctions inner)
        {
            _inner = inner;
        }

        public Dictionary<string, FileInfo> GetFiles(LocalPath localPath)
        {
            var files = _inner.GetFiles(localPath);
            Log.PutOut($"FileInfo dictionary received. (LocalPath: {localPath})");
            return files;
        }

        public bool EqualSize(S3ObjectInfo s3Object, FileInfo fileInfo)
        {
            if (_inner.EqualSize(s3Object, fileInfo))
            {
                Log.PutOut($"Size {s3Object.Key} {fileInfo.Name} matched.");
                return true;
            }

            Log.PutOut($"File {fileInfo.Name} size does not match S3Object {s3Object.Key}.");
            return false;
        }

        public bool EqualETag(S3ObjectInfo s3Object, FileInfo fileInfo, PartSize partSize)
        {
            if (_inner.EqualETag(s3Object, fileInfo, partSize))
            {
                Log.PutOut($"Hash {s3Object.Key} {fileInfo.Name} matched.");
                return true;
            }

            Log.PutOut($"File {fileInfo.Name} hash does not match S3Object {s3Object.Key}.");
            return false;
        }
    }
}
