using System;

using S3Backup.Components;

namespace S3Backup
{

    public sealed class RecycleAge : RealNumberOption<RecycleAge>
    {
        public const int MinValue = 1;
        public const int MaxValue = 366;

        public static readonly RecycleAge Min = new RecycleAge(MinValue);

        public static readonly RecycleAge Max = new RecycleAge(MaxValue);

        public RecycleAge(int value)
             : base(value, MinValue, MaxValue)
        {
        }

        public static DateTime ParseToDateTime(RecycleAge value)
        {
            return DateTime.Now.Subtract(new TimeSpan(value, 0, 0, 0));
        }
    }
}
