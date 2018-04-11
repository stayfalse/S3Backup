namespace S3Backup.Components
{
    public abstract class RealNumberOption<TDomain> : ComparableDomain<TDomain, int>
        where TDomain : ComparableDomain<TDomain, int>
    {
        protected RealNumberOption(int value, int minvalue = RealNumberOption.AbsoluteMinValue, int maxvalue = RealNumberOption.AbsoluteMaxValue)
                : base(RealNumberOption.Initialize(value, minvalue, maxvalue))
        {
        }
    }
}
