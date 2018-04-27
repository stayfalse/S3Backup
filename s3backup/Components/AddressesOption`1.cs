namespace S3Backup.Components
{
    public delegate bool Validator(string path);

    public abstract class AddressesOption<TDomain> : EquatableDomain<TDomain, string>
    where TDomain : EquatableDomain<TDomain, string>
    {
        protected AddressesOption(string value, Validator validator)
            : base(AddressesOption.Initialize(value, validator))
        {
        }

        public int Length => Value.Length;
    }
}
