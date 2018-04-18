using System;

namespace S3Backup
{
    public class LogMessageDecorator<T> : DecoratorBase<ILog<T>>, ILog<T>
        where T : class
    {
        public LogMessageDecorator(ILog<T> log)
        : base(log)
        {
        }

        public void PutError(FormattableString data)
        {
            Inner.PutError($"{DateTime.UtcNow:o} {data}");
        }

        public void PutOut(FormattableString data)
        {
            Inner.PutOut($"{DateTime.UtcNow:o} {data}");
        }
    }
}
