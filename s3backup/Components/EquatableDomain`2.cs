using System;

namespace S3Backup.Components
{

    public class EquatableDomain<TDomain, TValue> : Domain<TDomain, TValue>, IEquatable<TDomain>
        where TDomain : EquatableDomain<TDomain, TValue>
        where TValue : IEquatable<TValue>
    {
        public EquatableDomain(TValue value)
            : base(value)
        {
        }

        public static bool operator ==(EquatableDomain<TDomain, TValue> x, TDomain y)
        {
            if (x is null)
            {
                return y is null;
            }

            return x.Equals(y);
        }

        public static bool operator !=(EquatableDomain<TDomain, TValue> x, TDomain y) => !(x == y);

        public static bool operator ==(EquatableDomain<TDomain, TValue> x, TValue y) // !!!!!!!!!
        {
            if (x is null)
            {
                return Types.IsNull(y);
            }

            return x.Value.Equals(y);
        }

        public static bool operator !=(EquatableDomain<TDomain, TValue> x, TValue y) => !(x == y);

        public static bool operator ==(TValue x, EquatableDomain<TDomain, TValue> y)
        {
            if (Types.IsNull(x))
            {
                return y is null;
            }

            if (y is null)
            {
                return false;
            }

            return x.Equals(y.Value);
        }

        public static bool operator !=(TValue x, EquatableDomain<TDomain, TValue> y) => !(x == y);

        public override bool Equals(object obj) => Equals(obj as TDomain);

        public override int GetHashCode() => Value.GetHashCode();

        public virtual bool Equals(TDomain other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (other is null)
            {
                return false;
            }

            var equals = Value.Equals(other.Value);
            return equals;
        }
    }
}
