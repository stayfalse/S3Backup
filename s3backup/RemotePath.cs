using S3Backup.Components;

namespace S3Backup
{
    public sealed class RemotePath : AddressesOption<RemotePath>
    {
        public RemotePath(string value)
             : base(value, (string path) => System.Text.Encoding.UTF8.GetByteCount(path) <= 1024)
        {
        }
    }
}
