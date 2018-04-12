using System;

namespace S3Backup.Components
{
    public abstract class ThresholdDateOption<TDomain> : ComparableDomain<TDomain, DateTime>
    where TDomain : ComparableDomain<TDomain, DateTime>
    {
        protected ThresholdDateOption(DateTime value, int minvalue = ThresholdDateOption.AbsoluteMinRecycleAge, int maxvalue = ThresholdDateOption.AbsoluteMaxRecycleAge)
                : base(ThresholdDateOption.Initialize(value, minvalue, maxvalue))
        {
        }
    }
}
