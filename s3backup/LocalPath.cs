using S3Backup.Components;

namespace S3Backup
{
    public sealed class LocalPath : AddressesOption<LocalPath>
    {
        public const int MaxLength = 100;

        public LocalPath(string value)
             : base(value, (string path) => Validate(path), maxLength: MaxLength)
        {
        }

        private static bool Validate(string path)
        {
            try
            {
                System.IO.Path.GetFullPath(path);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
