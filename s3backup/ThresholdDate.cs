using System;

using S3Backup.Components;

namespace S3Backup
{
    public sealed class ThresholdDate : ThresholdDateOption<ThresholdDate>
    {
        public const int MinRecycleAge = 1;
        public const int MaxRecycleAge = 366;

        public static readonly ThresholdDate Min = new ThresholdDate(DateTime.Now.Subtract(new TimeSpan(MinRecycleAge, 0, 0, 0)));

        public static readonly ThresholdDate Max = new ThresholdDate(DateTime.Now.Subtract(new TimeSpan(MaxRecycleAge, 0, 0, 0)));

        public ThresholdDate(DateTime value)
            : base(value, MinRecycleAge, MaxRecycleAge)
        {
        }
    }
}
