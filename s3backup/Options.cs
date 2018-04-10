namespace S3Backup
{
    public sealed class Options
    {
        public Options(bool illegalArgumet, OptionCases optionCases, LocalPath localPath, RemotePath remotePath, PartSize partSize, RecycleAge recycleAge, ParallelParts paralellParts)
        {
            IllegalArgument = illegalArgumet;

            OptionCases = optionCases;
            LocalPath = localPath;
            RemotePath = remotePath;
            PartSize = partSize;
            RecycleAge = recycleAge;
            ParallelParts = paralellParts;
        }

        public bool IllegalArgument { get; private set; }

        public PartSize PartSize { get; private set; }

        public OptionCases OptionCases { get; private set; }

        public ParallelParts ParallelParts { get; private set; }

        public RecycleAge RecycleAge { get; private set; }

        public LocalPath LocalPath { get; private set; }

        public RemotePath RemotePath { get; private set; }
    }

    public sealed class AmazonOptions
    {
        public AmazonOptions(ClientInformation clientInformation, BucketName bucketName)
        {
            ClientInformation = clientInformation;
            BucketName = bucketName;
        }

        public ClientInformation ClientInformation { get; private set; }

        public BucketName BucketName { get; private set; }
    }
}
