using S3Backup.Components;

namespace S3Backup
{
    public sealed class LogFilePath : AddressesOption<LocalPath>
    {
        public LogFilePath(string value)
            : base(value, (string path) => Validate(path))
        {
        }

        private static bool Validate(string path)
        {
            try
            {
                System.IO.Path.GetFullPath(path);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
