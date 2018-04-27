using System;

namespace S3Backup
{
    public sealed class AmazonOptions
    {
        public AmazonOptions(ClientInformation clientInformation, BucketName bucketName, ParallelParts parallelParts, bool dryRun)
        {
            ClientInformation = clientInformation ?? throw new ArgumentNullException(nameof(clientInformation));
            BucketName = bucketName ?? throw new ArgumentNullException(nameof(bucketName));
            ParallelParts = parallelParts ?? throw new ArgumentNullException(nameof(parallelParts));
            DryRun = dryRun;
        }

        public ClientInformation ClientInformation { get; }

        public BucketName BucketName { get; }

        public ParallelParts ParallelParts { get; }

        public bool DryRun { get; }
    }
}
