using System;

namespace S3Backup.Components
{
    public static class AddressesOption
    {
        public const int AbsoluteMaxLength = 100;

        internal static string Initialize(string value, Validator validator, int maxLength)
        {
            if (maxLength > AbsoluteMaxLength)
            {
                throw new InvalidOperationException($"Incorrect length {maxLength}.");
            }

            if (value.Length > maxLength)
            {
                throw new ArgumentOutOfRangeException($"{nameof(value)} {value.Length} Value length is greater than {maxLength}.");
            }

            if (!validator(value))
            {
                throw new ArgumentOutOfRangeException($"{value} is not valid address.");
            }

            return value;
        }
    }
}
