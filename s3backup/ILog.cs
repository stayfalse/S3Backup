using System;
using System.IO;

namespace S3Backup
{
    public interface ILog<AnyLogger>
    {
        void PutError(FormattableString data);

        void PutOut(FormattableString data);
    }

    public class FileLog : ILog<FileLog>
    {
        private const string _path = "log.txt";
        private readonly StreamWriter _streamWriter = File.AppendText(_path);

        public void PutError(FormattableString data)
        {
            _streamWriter.WriteLine(FormattableString.Invariant(data));
        }

        public void PutOut(FormattableString data)
        {
            _streamWriter.WriteLine(FormattableString.Invariant(data));
            _streamWriter.Flush();
        }
    }

    public class CombinedLog : ILog<CombinedLog>
    {
        private readonly ILog<FileLog> _fileLog;
        private readonly ILog<ConsoleLog> _consoleLog;

        public CombinedLog(ILog<FileLog> fileLog, ILog<ConsoleLog> consoleLog)
        {
            _fileLog = fileLog;
            _consoleLog = consoleLog;
        }

        public void PutError(FormattableString data)
        {
            _fileLog.PutError(data);
            _consoleLog.PutError(data);
        }

        public void PutOut(FormattableString data)
        {
            _fileLog.PutOut(data);
            _consoleLog.PutOut(data);
        }
    }

    public class LogMessageDecorator<T> : ILog<T>
        where T : ILog<T>
    {
        private readonly ILog<T> _inner;

        public LogMessageDecorator(ILog<T> log)
        {
            _inner = log;
        }

        public void PutError(FormattableString data)
        {
            _inner.PutError($"{DateTime.UtcNow:o} {data}");
        }

        public void PutOut(FormattableString data)
        {
            _inner.PutOut($"{DateTime.UtcNow:o} {data}");
        }
    }
}
