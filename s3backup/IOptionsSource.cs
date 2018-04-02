using System;
using System.Collections.Generic;
using System.Text;

namespace S3Backup
{
    public interface IOptionsSource
    {
        Options Options { get; }
    }
}
