using System;

namespace S3Backup.Components
{
    public static class ThresholdDateOption
    {
        public const int AbsoluteMinRecycleAge = 1;
        public const int AbsoluteMaxRecycleAge = 366;

        internal static DateTime Initialize(DateTime value, int minvalue, int maxvalue)
        {
            if (minvalue < AbsoluteMinRecycleAge || maxvalue > AbsoluteMaxRecycleAge || minvalue > maxvalue)
            {
                throw new InvalidOperationException($"Invalid range [{minvalue};{maxvalue}] days age.");
            }

            var valueTimeSpan = DateTime.Now - value;
            if (value != default && (valueTimeSpan < new TimeSpan(minvalue, 0, 0, 0) || valueTimeSpan > new TimeSpan(maxvalue, 0, 0, 0)))
            {
                throw new ArgumentOutOfRangeException($"{nameof(value)} {value} Value is out of range [{minvalue};{maxvalue}] days age.");
            }

            return value;
        }
    }
}
