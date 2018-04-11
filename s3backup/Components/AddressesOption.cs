using System;

namespace S3Backup.Components
{
    public static class AddressesOption
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
}
