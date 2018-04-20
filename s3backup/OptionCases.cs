using System;

namespace S3Backup
{
    [Flags]
    public enum OptionCases : ushort
    {
        None,
        Purge = 1,
        SizeOnly = 2,
    }
}
