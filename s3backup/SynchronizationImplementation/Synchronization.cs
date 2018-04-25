using System;
using System.Threading.Tasks;

namespace S3Backup.SynchronizationImplementation
{
    public class Synchronization : ISynchronization
    {
        private readonly ISynchronizationFunctions _synchronizationFunctions;

        public Synchronization(ISynchronizationFunctions synchronizationFunctions)
        {
            _synchronizationFunctions = synchronizationFunctions ?? throw new ArgumentNullException(nameof(synchronizationFunctions));
        }

        public async Task Synchronize()
        {
            await _synchronizationFunctions.Purge().ConfigureAwait(false);

            var objects = await _synchronizationFunctions.GetObjectsList().ConfigureAwait(false);
            var filesInfo = _synchronizationFunctions.GetFilesDictionary();
            foreach (var s3Object in objects)
            {
                if (filesInfo.TryGetValue(s3Object.Key, out var fileInfo))
                {
                    filesInfo.Remove(fileInfo.Name);
                    if (!_synchronizationFunctions.FileEqualsObject(fileInfo, s3Object))
                    {
                        await _synchronizationFunctions.UploadMismatchedFile(fileInfo).ConfigureAwait(false);
                    }
                }
                else
                {
                    await _synchronizationFunctions.DeleteExcessObject(s3Object).ConfigureAwait(false);
                }
            }

            await _synchronizationFunctions.UploadMissingFiles(filesInfo.Values).ConfigureAwait(false);
        }
    }
}
