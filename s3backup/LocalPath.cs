using System;

using S3Backup.Components;

namespace S3Backup
{

    public sealed class LocalPath : AdresssOption<LocalPath>
    {
        public const int MaxLength = 100;

        public LocalPath(string value)
             : base(value, MaxLength, System.IO.Directory.Exists(value))
        {
        }
    }
}
