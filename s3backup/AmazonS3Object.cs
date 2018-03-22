using System;

using Amazon.S3.Model;

namespace S3Backup
{
    public class AmazonS3Object : IS3Object
    {
        private string _key;
        private long _size;
        private string _eTag;
        private DateTime _lastModified;

        public AmazonS3Object(S3Object s3Object)
        {
            _key = s3Object.Key;
            _size = s3Object.Size;
            _eTag = s3Object.ETag;
            _lastModified = s3Object.LastModified;
        }

        public string Key => _key;

        public long Size => _size;

        public string ETag => _eTag;

        public DateTime LastModified => _lastModified;
    }
}
