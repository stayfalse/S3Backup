using System;

namespace S3Backup.Logging
{
    public interface ILog<T>
        where T : class
    {
        void PutError(FormattableString data);

        void PutOut(FormattableString data);
    }
}
