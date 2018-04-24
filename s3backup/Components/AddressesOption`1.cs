namespace S3Backup.Components
{
    public delegate bool Validator(string path);

    public abstract class AddressesOption<TDomain> : EquatableDomain<TDomain, string>
    where TDomain : EquatableDomain<TDomain, string>
    {
        protected AddressesOption(string value, Validator validator, int maxLength = AddressesOption.AbsoluteMaxLength)
            : base(AddressesOption.Initialize(value, validator, maxLength))
        {
        }

        public int Length => Value.Length;
    }
}
