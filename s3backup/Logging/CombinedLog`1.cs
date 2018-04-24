using System;

namespace S3Backup.Logging
{
    public class CombinedLog<T> : ILog<T>
        where T : class
    {
        private readonly IFileLog _fileLog;
        private readonly IConsoleLog _consoleLog;

        public CombinedLog(IFileLog fileLog, IConsoleLog consoleLog)
        {
            var f = fileLog as DecoratorBase<IFileLog>;
            _fileLog = (f != null) ? f.GetComponent() : fileLog ?? throw new ArgumentNullException(nameof(fileLog));
            var c = consoleLog as DecoratorBase<IConsoleLog>;
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
