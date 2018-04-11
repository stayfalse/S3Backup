namespace S3Backup
{
    public sealed class Options
    {
        public Options(bool illegalArgument, OptionCases optionCases, LocalPath localPath, RemotePath remotePath, PartSize partSize, RecycleAge recycleAge, ParallelParts paralellParts)
        {
            IllegalArgument = illegalArgument;

            OptionCases = optionCases;
            LocalPath = localPath;
            RemotePath = remotePath;
            PartSize = partSize;
            RecycleAge = recycleAge;
            ParallelParts = paralellParts;
        }

        public bool IllegalArgument { get; }

        public PartSize PartSize { get; }

        public OptionCases OptionCases { get; }

        public ParallelParts ParallelParts { get; }

        public RecycleAge RecycleAge { get; }

        public LocalPath LocalPath { get; }

        public RemotePath RemotePath { get; }
    }
}
