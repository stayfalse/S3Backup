﻿using System;

using S3Backup.Components;

namespace S3Backup
{

    [Flags]
    public enum OptionCases
    {
        None,
        DryRun = 1,
        Purge = 2,
        SizeOnly = 4,
    }
}
