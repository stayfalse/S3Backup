using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace S3Backup.Components
{
    public static class Types
    {
        public static bool IsNull<T>(T value)
        {
            var type = typeof(T);
            if (!type.IsValueType)
            {
                return ReferenceEquals(value, null);
            }

            if (type.IsConstructedGenericType)
            {
                var genericType = type.GetGenericTypeDefinition();
                if (genericType == typeof(Nullable<>))
                {
                    return value.Equals(null);
                }

                if (genericType == typeof(ImmutableArray<>))
                {
                    return EqualityComparer<T>.Default.Equals(value, default);
                }
            }

            return false;
        }

        public static bool IsNull<T>(T? value)
            where T : struct
            => !value.HasValue;
    }
}
