using System;
using System.Threading.Tasks;

using S3Backup.Logging;

namespace S3Backup.SynchronizationImplementation
{
    public class SynchronizationLoggingDecorator : ISynchronization
    {
        private readonly ISynchronization _inner;
        private readonly ILog<ISynchronization> _log;

        public SynchronizationLoggingDecorator(ISynchronization synchronization, ILog<ISynchronization> log)
        {
            _inner = synchronization ?? throw new ArgumentNullException(nameof(synchronization));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public async Task Synchronize()
        {
            try
            {
                _log.PutOut($"Synchronization started.");
                await _inner.Synchronize().ConfigureAwait(false);
                _log.PutOut($"Synchronization completed.");
            }
            catch (Exception exception)
            {
                _log.PutError($"Exception occurred: {exception.Message}.");
                _log.PutOut($"Synchronization can not be completed in case of following exception : {exception.Message}.");
            }
        }
    }
}
