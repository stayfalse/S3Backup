namespace S3Backup
{
    public sealed class Options
    {
        private readonly bool _illegalArgument;
        private readonly OptionCases _optionCases;
        private readonly PartSize _partSize;
        private readonly ParallelParts _parallelParts;
        private readonly RecycleAge _recycleAge;
        private readonly LocalPath _localPath;
        private readonly RemotePath _remotePath;

        public Options(bool illegalArgumet, OptionCases optionCases, LocalPath localPath, RemotePath remotePath, PartSize partSize, RecycleAge recycleAge, ParallelParts paralellParts)
        {
            _illegalArgument = illegalArgumet;

            _optionCases = optionCases;
            _localPath = localPath;
            _remotePath = remotePath;
            _partSize = partSize;
            _recycleAge = recycleAge;
            _parallelParts = paralellParts;
        }

        public bool IllegalArgument => _illegalArgument;

        public PartSize PartSize => _partSize;

        public OptionCases OptionCases => _optionCases;

        public ParallelParts ParallelParts => _parallelParts;

        public RecycleAge RecycleAge => _recycleAge;

        public LocalPath LocalPath => _localPath;

        public RemotePath RemotePath => _remotePath;
    }
}
