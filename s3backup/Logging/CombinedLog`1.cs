using System;

namespace S3Backup.Logging
{
    public class CombinedLog<T> : ILog<T>
        where T : class
    {
        private readonly ILog<FileLog> _fileLog;
        private readonly ILog<ConsoleLog> _consoleLog;

        public CombinedLog(ILog<FileLog> fileLog, ILog<ConsoleLog> consoleLog)
        {
            var f = fileLog as DecoratorBase<ILog<FileLog>>;
            _fileLog = (f != null) ? f.GetComponent() : fileLog ?? throw new ArgumentNullException(nameof(fileLog));
            var c = consoleLog as DecoratorBase<ILog<ConsoleLog>>;
            _consoleLog = (c != null) ? c.GetComponent() : consoleLog ?? throw new ArgumentNullException(nameof(consoleLog));
        }

        private enum ConsoleLogClasses
        {
            ISynchronization,
            IAmazonFunctionsDryRunChecker,
        }

        public void PutError(FormattableString data)
        {
            _fileLog.PutError(data);
            _consoleLog.PutError(data);
        }

        public void PutOut(FormattableString data)
        {
            _fileLog.PutOut(data);
            if (Enum.TryParse(typeof(ConsoleLogClasses), typeof(T).Name, true, out var t))
            {
                _consoleLog.PutOut(data);
            }
        }
    }
}
