using System.Threading.Tasks;

namespace S3Backup.SynchronizationImplementation
{
    public interface ISynchronization
    {
        Task Synchronize();
    }
}
