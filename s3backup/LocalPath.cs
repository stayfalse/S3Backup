using S3Backup.Components;

namespace S3Backup
{
    public sealed class LocalPath : AddressesOption<LocalPath>
    {
        public const int MaxLength = 100;

        public LocalPath(string value)
             : base(value, (string path) => System.IO.Directory.Exists(path), MaxLength)
        {
        }
    }
}
