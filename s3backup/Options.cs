using System;

namespace S3Backup
{
    public sealed class Options
    {
        public Options(OptionCases optionCases, LocalPath localPath, RemotePath remotePath, PartSize partSize, ThresholdDate threshold, ParallelParts paralellParts)
        {
            OptionCases = optionCases;
            LocalPath = localPath ?? throw new ArgumentNullException(nameof(localPath));
            RemotePath = remotePath ?? throw new ArgumentNullException(nameof(remotePath));
            PartSize = partSize ?? throw new ArgumentNullException(nameof(partSize));
            ThresholdDate = threshold ?? throw new ArgumentNullException(nameof(threshold));
            ParallelParts = paralellParts ?? throw new ArgumentNullException(nameof(paralellParts));
        }

        public PartSize PartSize { get; }

        public OptionCases OptionCases { get; }

        public ParallelParts ParallelParts { get; }

        public ThresholdDate ThresholdDate { get; }

        public LocalPath LocalPath { get; }

        public RemotePath RemotePath { get; }
    }
}
