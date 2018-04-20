using System;

namespace S3Backup
{
    public sealed class AmazonOptions
    {
        public AmazonOptions(ClientInformation clientInformation, BucketName bucketName, bool dryRun)
        {
            ClientInformation = clientInformation ?? throw new ArgumentNullException(nameof(clientInformation));
            BucketName = bucketName ?? throw new ArgumentNullException(nameof(bucketName));
            DryRun = dryRun;
        }

        public ClientInformation ClientInformation { get; }

        public BucketName BucketName { get; }

        public bool DryRun { get; }
    }
}
