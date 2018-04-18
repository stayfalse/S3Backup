using System;

namespace S3Backup.Components
{
    public class Domain<TDomain, TValue>
        where TDomain : Domain<TDomain, TValue>
    {
        private static readonly Func<TValue> DefaultValueProvider =
            typeof(TValue).IsValueType && Nullable.GetUnderlyingType(typeof(TValue)) == null
                ? (Func<TValue>)ThrowNull
                : () => default;

        private readonly TValue _value;

        public Domain(TValue value)
        {
            if (Types.IsNull(value))
            {
                throw new ArgumentNullException(nameof(value));
            }

            _value = value;
        }

        protected TValue Value => _value;

        public static implicit operator TValue(Domain<TDomain, TValue> domain) => domain._value;

        public virtual string ToString(string format, IFormatProvider formatProvider)
        {
            if (Value is IFormattable formattableValue)
            {
                return formattableValue.ToString(format, formatProvider);
            }

            if (Value is IConvertible convertibleValue)
            {
                return convertibleValue.ToString(formatProvider);
            }

            return Value.ToString();
        }

        public TypeCode GetTypeCode() => (Value as IConvertible)?.GetTypeCode() ?? TypeCode.Empty;

        public bool ToBoolean(IFormatProvider provider) => Convert(convertible => convertible.ToBoolean(provider));

        public char ToChar(IFormatProvider provider) => Convert(convertible => convertible.ToChar(provider));

        public sbyte ToSByte(IFormatProvider provider) => Convert(convertible => convertible.ToSByte(provider));

        public byte ToByte(IFormatProvider provider) => Convert(convertible => convertible.ToByte(provider));

        public short ToInt16(IFormatProvider provider) => Convert(convertible => convertible.ToInt16(provider));

        public ushort ToUInt16(IFormatProvider provider) => Convert(convertible => convertible.ToUInt16(provider));

        public int ToInt32(IFormatProvider provider) => Convert(convertible => convertible.ToInt32(provider));

        public uint ToUInt32(IFormatProvider provider) => Convert(convertible => convertible.ToUInt32(provider));

        public long ToInt64(IFormatProvider provider) => Convert(convertible => convertible.ToInt64(provider));

        public ulong ToUInt64(IFormatProvider provider) => Convert(convertible => convertible.ToUInt64(provider));

        public float ToSingle(IFormatProvider provider) => Convert(convertible => convertible.ToSingle(provider));

        public double ToDouble(IFormatProvider provider) => Convert(convertible => convertible.ToDouble(provider));

        public decimal ToDecimal(IFormatProvider provider) => Convert(convertible => convertible.ToDecimal(provider));

        public DateTime ToDateTime(IFormatProvider provider) => Convert(convertible => convertible.ToDateTime(provider));

        public string ToString(IFormatProvider provider) => ToString(null, provider);

        public object ToType(Type conversionType, IFormatProvider provider) => Convert(convertible => convertible.ToType(conversionType, provider));

        private static TValue ThrowNull()
        {
            throw new ArgumentNullException("domain", "A null domain " + typeof(TDomain) + " was encountered.");
        }

        private T Convert<T>(Func<IConvertible, T> converter)
        {
            if (Value is IConvertible convertible)
            {
                return converter(convertible);
            }

            throw new InvalidCastException("Cannot convert domain " + typeof(TDomain) + " to " + typeof(T) + ".");
        }
    }
}
