using System;
using System.IO;

namespace S3Backup
{
    public class FileLog : ILog<FileLog>
    {
        private const string _path = "log.txt";

        public void PutError(FormattableString data)
        {
            using (var streamWriter = File.AppendText(_path))
            {
                streamWriter.WriteLine(FormattableString.Invariant(data));
            }
        }

        public void PutOut(FormattableString data)
        {
            using (var streamWriter = File.AppendText(_path))
            {
                streamWriter.WriteLine(FormattableString.Invariant(data));
            }
        }
    }
}
