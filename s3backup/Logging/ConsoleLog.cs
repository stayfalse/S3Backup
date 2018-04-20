using System;

using static System.Console;

namespace S3Backup.Logging
{
    public class ConsoleLog : ILog<ConsoleLog>
    {
        public void PutOut(FormattableString data)
        {
            Out.WriteLine(FormattableString.Invariant(data));
        }

        public void PutError(FormattableString data)
        {
            Error.WriteLine(FormattableString.Invariant(data));
        }
    }
}
