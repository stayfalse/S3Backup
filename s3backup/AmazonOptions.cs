namespace S3Backup
{
    public sealed class AmazonOptions
    {
        private readonly ClientInformation _clientInformation;
        private readonly BucketName _bucketName;

        public AmazonOptions(ClientInformation clientInformation, BucketName bucketName)
        {
            _clientInformation = clientInformation;
            _bucketName = bucketName;
        }

        public ClientInformation ClientInformation => _clientInformation;

        public BucketName BucketName => _bucketName;
    }
}
