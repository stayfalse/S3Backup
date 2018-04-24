using System;

using S3Backup.Logging;

namespace S3Backup.Composition
{
    public class ArgsParserExceptionHandler : IArgsParser
    {
        private readonly IArgsParser _inner;
        private readonly ILog<IArgsParser> _log;

        public ArgsParserExceptionHandler(IArgsParser argsParser, ILog<IArgsParser> log)
        {
            _inner = argsParser ?? throw new ArgumentNullException(nameof(argsParser));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public (Options, AmazonOptions) Parse(string[] args)
        {
            try
            {
                return _inner.Parse(args);
            }
            catch (Exception exception)
            {
                _log.PutError($"Exception occurred: {exception.Message}");
                throw;
            }
        }
    }
}
