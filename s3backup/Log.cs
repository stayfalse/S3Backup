using System;
using static System.Console;

namespace S3Backup
{
    class Log
    {
        public static void PutOut(string data)
        {
            Out.WriteLine($"{DateTime.UtcNow.ToString("s", System.Globalization.CultureInfo.InvariantCulture)} {data}");
        }
        public static void PutError(string data)
        {
            Error.WriteLine($"{DateTime.UtcNow.ToString("s", System.Globalization.CultureInfo.InvariantCulture)} {data}");
        }

    }
}
