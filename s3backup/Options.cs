using System;

namespace S3Backup
{
    public sealed class Options
    {
        public Options(bool illegalArgument, OptionCases optionCases, LocalPath localPath, RemotePath remotePath, PartSize partSize, RecycleAge recycleAge, ParallelParts paralellParts)
        {
            IllegalArgument = illegalArgument;

            OptionCases = optionCases;
            LocalPath = localPath ?? throw new ArgumentNullException(nameof(localPath));
            RemotePath = remotePath ?? throw new ArgumentNullException(nameof(remotePath));
            PartSize = partSize ?? throw new ArgumentNullException(nameof(partSize));
            RecycleAge = recycleAge ?? throw new ArgumentNullException(nameof(recycleAge));
            ParallelParts = paralellParts ?? throw new ArgumentNullException(nameof(paralellParts));
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
