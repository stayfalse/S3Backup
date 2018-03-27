using System;

using static System.Console;

namespace S3Backup
{
    public static class Log
    {
        public static void PutOut(FormattableString data)
        {
            Out.WriteLine(FormattableString.Invariant($"{DateTime.UtcNow:o} {data}"));
        }

        public static void PutError(FormattableString data)
        {
            Error.WriteLine(FormattableString.Invariant($"{DateTime.UtcNow:o} {data}"));
        }
    }
}
