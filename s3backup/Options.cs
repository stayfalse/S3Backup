namespace S3Backup
{
    public sealed class Options
    {
        public Options(bool illegalArgumet, bool dryRun, bool sizeOnly, bool purge, string bucketName, string localPath, string remotePath, int partSize, int recycleAge, int paralellParts, ClientInformation clientInfo)
        {
            IllegalArgument = illegalArgumet;
            DryRun = dryRun;
            SizeOnly = sizeOnly;
            Purge = purge;
            BucketName = bucketName;
            LocalPath = localPath;
            RemotePath = remotePath;
            PartSize = partSize;
            RecycleAge = recycleAge;
            ParallelParts = paralellParts;
            ClientInformation = clientInfo;
        }

        public bool IllegalArgument { get; private set; }

        public int PartSize { get; private set; }

        public bool DryRun { get; private set; }

        public bool Purge { get; private set; }

        public bool SizeOnly { get; private set; }

        public int ParallelParts { get; private set; }

        public int RecycleAge { get; private set; }

        public string LocalPath { get; private set; }

        public string BucketName { get; private set; }

        public string RemotePath { get; private set; }

        public ClientInformation ClientInformation { get; private set; }
    }
}
