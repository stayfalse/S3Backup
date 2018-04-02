using System;
using System.Collections.Generic;

namespace S3Backup
{
    public interface IArgsParser
    {
        Options Parse(string[] args);
    }
}
