using S3Backup.Components;

namespace S3Backup
{
    public sealed class ParallelParts : RealNumberOption<ParallelParts>
    {
        public const int MinValue = 1;
        public const int MaxValue = 64;

        public static readonly ParallelParts Min = new ParallelParts(MinValue);

        public static readonly ParallelParts Max = new ParallelParts(MaxValue);

        public ParallelParts(int value)
             : base(value, MinValue, MaxValue)
        {
        }
    }
}
