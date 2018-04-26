using System;

namespace S3Backup.Logging
{
    public class CombinedLog<T> : ILog<T>
        where T : class
    {
        private readonly IFileLog _fileLog;
        private readonly IConsoleLog _consoleLog;
        private readonly bool _verbose;

        public CombinedLog(IFileLog fileLog, IConsoleLog consoleLog, IOptionsSource optionsSource)
        {
            if (optionsSource is null)
            {
                throw new ArgumentNullException(nameof(optionsSource));
            }

            var f = fileLog as DecoratorBase<IFileLog>;
            _fileLog = (f != null) ? f.GetComponent() : fileLog ?? throw new ArgumentNullException(nameof(fileLog));
            var c = consoleLog as DecoratorBase<IConsoleLog>;
            _consoleLog = (c != null) ? c.GetComponent() : consoleLog ?? throw new ArgumentNullException(nameof(consoleLog));
            _verbose = optionsSource.LogOptions.Verbose;
        }

        private enum ConsoleLogClasses
        {
            ISynchronization,
            ISynchronizationFunctions,
            IAmazonFunctionsDryRunChecker,
            IAmazonFunctions,
        }

        public void PutError(FormattableString data)
        {
            _fileLog.PutError(data);
            _consoleLog.PutError(data);
        }

        public void PutOut(FormattableString data)
        {
            _fileLog.PutOut(data);
            if (_verbose || Enum.TryParse(typeof(ConsoleLogClasses), typeof(T).Name, true, out var t))
            {
                _consoleLog.PutOut(data);
            }
        }
    }
}
