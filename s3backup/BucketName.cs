using System.Linq;

using S3Backup.Components;

namespace S3Backup
{
    public sealed class BucketName : AddressesOption<BucketName>
    {
        public const int MaxLength = 63;
        public const int MinLength = 3;

        public BucketName(string value)
             : base(value, (string name) => (System.Uri.CheckHostName(name) == System.UriHostNameType.Dns && !name.Any((char c) => char.IsUpper(c) || c == '_')), MinLength, MaxLength)
        {
        }
    }
}
