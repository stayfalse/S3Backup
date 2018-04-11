namespace S3Backup
{
    public sealed class AmazonOptions
    {
        public AmazonOptions(ClientInformation clientInformation, BucketName bucketName)
        {
            ClientInformation = clientInformation;
            BucketName = bucketName;
        }

        public ClientInformation ClientInformation { get; }

        public BucketName BucketName { get; }
    }
}
