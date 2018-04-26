using S3Backup.Components;

namespace S3Backup
{
    public sealed class RemotePath : AddressesOption<RemotePath>
    {
        public const int MaxLength = 100;

        public RemotePath(string value)
             : base(value, (string path) => true, maxLength: MaxLength)
        {
        }
    }
}
