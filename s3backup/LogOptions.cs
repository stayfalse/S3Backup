using System;

namespace S3Backup
{
    public sealed class LogOptions
    {
        public LogOptions(LogFilePath logFilePath, bool verbose)
        {
            LogFilePath = logFilePath ?? throw new ArgumentNullException(nameof(logFilePath));
            Verbose = verbose;
        }

        public LogFilePath LogFilePath { get; }

        public bool Verbose { get; }
    }
}
