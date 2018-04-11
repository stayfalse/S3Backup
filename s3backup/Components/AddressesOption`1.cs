namespace S3Backup.Components
{
    public abstract class AddressesOption<TDomain> : EquatableDomain<TDomain, string>
    where TDomain : EquatableDomain<TDomain, string>
    {
        protected AddressesOption(string value, int maxLength = AddressesOption.AbsoluteMaxLength, bool isValid = true)
            : base(AddressesOption.Initialize(value, maxLength, isValid))
        {
        }

        public int Length => Value.Length;
    }
}
