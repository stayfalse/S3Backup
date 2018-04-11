using System;

namespace S3Backup
{
    public sealed class AmazonOptions
    {
        public AmazonOptions(ClientInformation clientInformation, BucketName bucketName)
        {
            ClientInformation = clientInformation ?? throw new ArgumentNullException(nameof(clientInformation));
            BucketName = bucketName ?? throw new ArgumentNullException(nameof(bucketName));
        }

        public ClientInformation ClientInformation { get; }

        public BucketName BucketName { get; }
    }
}
