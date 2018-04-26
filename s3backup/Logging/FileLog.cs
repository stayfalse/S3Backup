using System;
using System.IO;

namespace S3Backup.Logging
{
    public class FileLog : IFileLog
    {
        private readonly LogFilePath _path;

        public FileLog(IOptionsSource optionsSource)
        {
            if (optionsSource is null)
            {
                throw new ArgumentNullException(nameof(optionsSource));
            }

            _path = optionsSource.LogOptions.LogFilePath;
        }

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
