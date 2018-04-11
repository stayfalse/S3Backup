using S3Backup.Components;

namespace S3Backup
{
    public sealed class PartSize : RealNumberOption<PartSize>
    {
        public const int MinValue = 1;
        public const int MaxValue = 1073741824;

        public static readonly PartSize Min = new PartSize(MinValue);

        public static readonly PartSize Max = new PartSize(MaxValue);

        public PartSize(int value)
             : base(value, MinValue, MaxValue)
        {
        }
    }
}
