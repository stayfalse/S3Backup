using System;

namespace S3Backup.Components
{
    public static class AddressesOption
    {
        internal static string Initialize(string value, Validator validator)
        {
            if (!validator(value))
            {
                throw new ArgumentOutOfRangeException($"{value} is not valid address.");
            }

            return value;
        }
    }
}
