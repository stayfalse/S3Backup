using System;

namespace S3Backup.Components
{
    public static class AddressesOption
    {
        public const int AbsoluteMaxLength = 100;
        public const int AbsoluteMinLength = 1;

        internal static string Initialize(string value, Validator validator, int minLength, int maxLength)
        {
            if (maxLength > AbsoluteMaxLength || minLength < AbsoluteMinLength || maxLength < minLength)
            {
                throw new InvalidOperationException($"Invalid range [{minLength};{maxLength}].");
            }

            if (value.Length > maxLength || value.Length < minLength)
            {
                throw new ArgumentOutOfRangeException($"{nameof(value)} {value.Length} Value length is out of range [{minLength};{maxLength}].");
            }

            if (!validator(value))
            {
                throw new ArgumentOutOfRangeException($"{value} is not valid address.");
            }

            return value;
        }
    }
}
