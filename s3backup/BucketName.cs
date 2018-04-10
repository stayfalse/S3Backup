using System;

using S3Backup.Components;

namespace S3Backup
{

    public sealed class BucketName : AdresssOption<BucketName>
    {
        public const int MaxLength = 100;

        public BucketName(string value)
             : base(value, MaxLength)
        {
        }
    }
}
