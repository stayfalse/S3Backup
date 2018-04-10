using System;

namespace S3Backup.Components
{
    public class ComparableDomain<TDomain, TValue> : EquatableDomain<TDomain, TValue>, IComparable<TDomain>
        where TDomain : ComparableDomain<TDomain, TValue>
        where TValue : IEquatable<TValue>, IComparable<TValue>
    {
        public ComparableDomain(TValue value)
            : base(value)
        {
        }

        public static bool operator <(ComparableDomain<TDomain, TValue> x, TDomain y)
        {
            if (x is null)
            {
                throw new ArgumentNullException(nameof(x));
            }

            return x.CompareTo(y) < 0;
        }

        public static bool operator <=(ComparableDomain<TDomain, TValue> x, TDomain y)
        {
            if (x is null)
            {
                throw new ArgumentNullException(nameof(x));
            }

            return x.CompareTo(y) <= 0;
        }

        public static bool operator >(ComparableDomain<TDomain, TValue> x, TDomain y)
        {
            if (x is null)
            {
                throw new ArgumentNullException(nameof(x));
            }

            return x.CompareTo(y) > 0;
        }

        public static bool operator >=(ComparableDomain<TDomain, TValue> x, TDomain y)
        {
            if (x is null)
            {
                throw new ArgumentNullException(nameof(x));
            }

            return x.CompareTo(y) >= 0;
        }

        public static bool operator <(ComparableDomain<TDomain, TValue> x, TValue y)
        {
            if (x is null)
            {
                throw new ArgumentNullException(nameof(x));
            }

            return x.Value.CompareTo(y) < 0;
        }

        public static bool operator <=(ComparableDomain<TDomain, TValue> x, TValue y)
        {
            if (x is null)
            {
                throw new ArgumentNullException(nameof(x));
            }

            return x.Value.CompareTo(y) <= 0;
        }

        public static bool operator >(ComparableDomain<TDomain, TValue> x, TValue y)
        {
            if (x is null)
            {
                throw new ArgumentNullException(nameof(x));
            }

            return x.Value.CompareTo(y) > 0;
        }

        public static bool operator >=(ComparableDomain<TDomain, TValue> x, TValue y)
        {
            if (x is null)
            {
                throw new ArgumentNullException(nameof(x));
            }

            return x.Value.CompareTo(y) >= 0;
        }

        public static bool operator <(TValue x, ComparableDomain<TDomain, TValue> y)
        {
            if (Types.IsNull(x))
            {
                throw new ArgumentNullException(nameof(x));
            }

            if (y is null)
            {
                throw new ArgumentNullException(nameof(y));
            }

            return x.CompareTo(y.Value) < 0;
        }

        public static bool operator <=(TValue x, ComparableDomain<TDomain, TValue> y)
        {
            if (Types.IsNull(x))
            {
                throw new ArgumentNullException(nameof(x));
            }

            if (y is null)
            {
                throw new ArgumentNullException(nameof(y));
            }

            return x.CompareTo(y.Value) <= 0;
        }

        public static bool operator >(TValue x, ComparableDomain<TDomain, TValue> y)
        {
            if (Types.IsNull(x))
            {
                throw new ArgumentNullException(nameof(x));
            }

            if (y is null)
            {
                throw new ArgumentNullException(nameof(y));
            }

            return x.CompareTo(y.Value) > 0;
        }

        public static bool operator >=(TValue x, ComparableDomain<TDomain, TValue> y)
        {
            if (Types.IsNull(x))
            {
                throw new ArgumentNullException(nameof(x));
            }

            if (y is null)
            {
                throw new ArgumentNullException(nameof(y));
            }

            return x.CompareTo(y.Value) >= 0;
        }

        public int CompareTo(TValue value) => CompareTo(value as TDomain);

        public virtual int CompareTo(TDomain other)
        {
            if (other is null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            return Value.CompareTo(other.Value);
        }
    }
}
