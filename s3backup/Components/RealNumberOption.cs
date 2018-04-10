using System;

namespace S3Backup.Components
{
    public static class RealNumberOption
    {
        public const int AbsoluteMinValue = 1;
        public const int AbsoluteMaxValue = int.MaxValue;

        internal static int Initialize(int value, int minvalue, int maxvalue)
        {
            if (minvalue < AbsoluteMinValue || maxvalue > AbsoluteMaxValue || minvalue > maxvalue)
            {
                throw new InvalidOperationException($"Invalid range [{minvalue};{maxvalue}].");
            }

            if (value < minvalue || value > maxvalue)
            {
                throw new ArgumentOutOfRangeException($"{nameof(value)} {value} Value is out of range [{minvalue};{maxvalue}].");
            }

            return value;
        }
    }

    public abstract class RealNumberOption<TDomain> : ComparableDomain<TDomain, int>
    where TDomain : ComparableDomain<TDomain, int>
    {
        protected RealNumberOption(int value, int minvalue = RealNumberOption.AbsoluteMinValue, int maxvalue = RealNumberOption.AbsoluteMaxValue)
                : base(RealNumberOption.Initialize(value, minvalue, maxvalue))
        {
        }
    }
}
