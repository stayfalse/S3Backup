using S3Backup.Components;

namespace S3Backup
{
    public sealed class BucketName : AddressesOption<BucketName>
    {
        public const int MaxLength = 100;

        public BucketName(string value)
             : base(value, (string path) => true, MaxLength)
        {
        }
    }
}
