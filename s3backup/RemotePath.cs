using System;

using S3Backup.Components;

namespace S3Backup
{

    public sealed class RemotePath : AdresssOption<RemotePath>
    {
        public const int MaxLength = 100;

        public RemotePath(string value)
             : base(value, MaxLength)
        {
        }
    }
}
