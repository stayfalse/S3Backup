using System;

namespace S3Backup.Components
{
    public static class AdressesOption
    {
        public const int AbsoluteMaxLength = 100;

        internal static string Initialize(string value, int maxLength, bool isValid)
        {
            if (maxLength > AbsoluteMaxLength)
            {
                throw new InvalidOperationException($"Incorrect length {maxLength}.");
            }

            if (value.Length > maxLength)
            {
                throw new ArgumentOutOfRangeException($"{nameof(value)} {value.Length} Value length is greater than {maxLength}.");
            }

            return value;
        }
    }

    public abstract class AdresssOption<TDomain> : EquatableDomain<TDomain, string>
    where TDomain : EquatableDomain<TDomain, string>
    {
        protected AdresssOption(string value, int maxLength = AdressesOption.AbsoluteMaxLength, bool isValid = true)
            : base(AdressesOption.Initialize(value, maxLength, isValid))
        {
        }
    }
}
