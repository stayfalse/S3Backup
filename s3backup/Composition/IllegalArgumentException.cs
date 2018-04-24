using System;
using System.Runtime.Serialization;

namespace S3Backup.Composition
{
    [Serializable]
    public class IllegalArgumentException : Exception
    {
        public IllegalArgumentException()
            : base("One of command line arguments is missing or invalid.")
        {
        }

        public IllegalArgumentException(string message)
            : base(message)
        {
        }

        public IllegalArgumentException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected IllegalArgumentException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
