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
            _log.PutOut($"Synchronization started");
            await _inner.Synchronize().ConfigureAwait(false);
            _log.PutOut($"Synchronization completed");
        }
    }
}
