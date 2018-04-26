namespace S3Backup.Components
{
    public delegate bool Validator(string path);

    public abstract class AddressesOption<TDomain> : EquatableDomain<TDomain, string>
    where TDomain : EquatableDomain<TDomain, string>
    {
        protected AddressesOption(string value, Validator validator, int minLength = AddressesOption.AbsoluteMinLength, int maxLength = AddressesOption.AbsoluteMaxLength)
            : base(AddressesOption.Initialize(value, validator, minLength, maxLength))
        {
        }

        public int Length => Value.Length;
    }
}
