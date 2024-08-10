using System;
using System.Collections.Generic;
using System.Linq;

namespace SpaceFox
{
    public static class EnumerableExtension
    {
        public static TValue GetMin<TValue>(this IEnumerable<TValue> collection, Func<TValue, float> marker)
            => GetExtremum(collection, marker, (a, b) => a < b);

        public static TValue GetMin<TValue>(this IEnumerable<TValue> collection, Func<TValue, double> marker)
            => GetExtremum(collection, marker, (a, b) => a < b);

        public static TValue GetMin<TValue>(this IEnumerable<TValue> collection, Func<TValue, decimal> marker)
            => GetExtremum(collection, marker, (a, b) => a < b);

        public static TValue GetMin<TValue>(this IEnumerable<TValue> collection, Func<TValue, sbyte> marker)
            => GetExtremum(collection, marker, (a, b) => a < b);

        public static TValue GetMin<TValue>(this IEnumerable<TValue> collection, Func<TValue, byte> marker)
            => GetExtremum(collection, marker, (a, b) => a < b);

        public static TValue GetMin<TValue>(this IEnumerable<TValue> collection, Func<TValue, short> marker)
            => GetExtremum(collection, marker, (a, b) => a < b);

        public static TValue GetMin<TValue>(this IEnumerable<TValue> collection, Func<TValue, ushort> marker)
            => GetExtremum(collection, marker, (a, b) => a < b);

        public static TValue GetMin<TValue>(this IEnumerable<TValue> collection, Func<TValue, int> marker)
            => GetExtremum(collection, marker, (a, b) => a < b);

        public static TValue GetMin<TValue>(this IEnumerable<TValue> collection, Func<TValue, uint> marker)
            => GetExtremum(collection, marker, (a, b) => a < b);

        public static TValue GetMin<TValue>(this IEnumerable<TValue> collection, Func<TValue, long> marker)
            => GetExtremum(collection, marker, (a, b) => a < b);

        public static TValue GetMin<TValue>(this IEnumerable<TValue> collection, Func<TValue, ulong> marker)
            => GetExtremum(collection, marker, (a, b) => a < b);

        public static TValue GetMin<TValue>(this IEnumerable<TValue> collection, Func<TValue, nint> marker)
            => GetExtremum(collection, marker, (a, b) => a < b);

        public static TValue GetMin<TValue>(this IEnumerable<TValue> collection, Func<TValue, nuint> marker)
            => GetExtremum(collection, marker, (a, b) => a < b);


        public static TValue GetMax<TValue>(this IEnumerable<TValue> collection, Func<TValue, float> marker)
            => GetExtremum(collection, marker, (a, b) => a > b);

        public static TValue GetMax<TValue>(this IEnumerable<TValue> collection, Func<TValue, double> marker)
            => GetExtremum(collection, marker, (a, b) => a > b);

        public static TValue GetMax<TValue>(this IEnumerable<TValue> collection, Func<TValue, decimal> marker)
            => GetExtremum(collection, marker, (a, b) => a > b);

        public static TValue GetMax<TValue>(this IEnumerable<TValue> collection, Func<TValue, sbyte> marker)
            => GetExtremum(collection, marker, (a, b) => a > b);

        public static TValue GetMax<TValue>(this IEnumerable<TValue> collection, Func<TValue, byte> marker)
            => GetExtremum(collection, marker, (a, b) => a > b);

        public static TValue GetMax<TValue>(this IEnumerable<TValue> collection, Func<TValue, short> marker)
            => GetExtremum(collection, marker, (a, b) => a > b);

        public static TValue GetMax<TValue>(this IEnumerable<TValue> collection, Func<TValue, ushort> marker)
            => GetExtremum(collection, marker, (a, b) => a > b);

        public static TValue GetMax<TValue>(this IEnumerable<TValue> collection, Func<TValue, int> marker)
            => GetExtremum(collection, marker, (a, b) => a > b);

        public static TValue GetMax<TValue>(this IEnumerable<TValue> collection, Func<TValue, uint> marker)
            => GetExtremum(collection, marker, (a, b) => a > b);

        public static TValue GetMax<TValue>(this IEnumerable<TValue> collection, Func<TValue, long> marker)
            => GetExtremum(collection, marker, (a, b) => a > b);

        public static TValue GetMax<TValue>(this IEnumerable<TValue> collection, Func<TValue, ulong> marker)
            => GetExtremum(collection, marker, (a, b) => a > b);

        public static TValue GetMax<TValue>(this IEnumerable<TValue> collection, Func<TValue, nint> marker)
            => GetExtremum(collection, marker, (a, b) => a > b);

        public static TValue GetMax<TValue>(this IEnumerable<TValue> collection, Func<TValue, nuint> marker)
            => GetExtremum(collection, marker, (a, b) => a > b);


        public static TValue GetMin<TValue>(this IEnumerable<TValue> collection, Func<TValue, float?> marker)
            => GetExtremum(collection, marker, (a, b) => a.HasValue && (!b.HasValue || a.Value < b.Value));

        public static TValue GetMin<TValue>(this IEnumerable<TValue> collection, Func<TValue, double?> marker)
            => GetExtremum(collection, marker, (a, b) => a.HasValue && (!b.HasValue || a.Value < b.Value));

        public static TValue GetMin<TValue>(this IEnumerable<TValue> collection, Func<TValue, decimal?> marker)
            => GetExtremum(collection, marker, (a, b) => a.HasValue && (!b.HasValue || a.Value < b.Value));

        public static TValue GetMin<TValue>(this IEnumerable<TValue> collection, Func<TValue, sbyte?> marker)
            => GetExtremum(collection, marker, (a, b) => a.HasValue && (!b.HasValue || a.Value < b.Value));

        public static TValue GetMin<TValue>(this IEnumerable<TValue> collection, Func<TValue, byte?> marker)
            => GetExtremum(collection, marker, (a, b) => a.HasValue && (!b.HasValue || a.Value < b.Value));

        public static TValue GetMin<TValue>(this IEnumerable<TValue> collection, Func<TValue, short?> marker)
            => GetExtremum(collection, marker, (a, b) => a.HasValue && (!b.HasValue || a.Value < b.Value));

        public static TValue GetMin<TValue>(this IEnumerable<TValue> collection, Func<TValue, ushort?> marker)
            => GetExtremum(collection, marker, (a, b) => a.HasValue && (!b.HasValue || a.Value < b.Value));

        public static TValue GetMin<TValue>(this IEnumerable<TValue> collection, Func<TValue, int?> marker)
            => GetExtremum(collection, marker, (a, b) => a.HasValue && (!b.HasValue || a.Value < b.Value));

        public static TValue GetMin<TValue>(this IEnumerable<TValue> collection, Func<TValue, uint?> marker)
            => GetExtremum(collection, marker, (a, b) => a.HasValue && (!b.HasValue || a.Value < b.Value));

        public static TValue GetMin<TValue>(this IEnumerable<TValue> collection, Func<TValue, long?> marker)
            => GetExtremum(collection, marker, (a, b) => a.HasValue && (!b.HasValue || a.Value < b.Value));

        public static TValue GetMin<TValue>(this IEnumerable<TValue> collection, Func<TValue, ulong?> marker)
            => GetExtremum(collection, marker, (a, b) => a.HasValue && (!b.HasValue || a.Value < b.Value));

        public static TValue GetMin<TValue>(this IEnumerable<TValue> collection, Func<TValue, nint?> marker)
            => GetExtremum(collection, marker, (a, b) => a.HasValue && (!b.HasValue || a.Value < b.Value));

        public static TValue GetMin<TValue>(this IEnumerable<TValue> collection, Func<TValue, nuint?> marker)
            => GetExtremum(collection, marker, (a, b) => a.HasValue && (!b.HasValue || a.Value < b.Value));


        public static TValue GetMax<TValue>(this IEnumerable<TValue> collection, Func<TValue, float?> marker)
            => GetExtremum(collection, marker, (a, b) => a.HasValue && (!b.HasValue || a.Value > b.Value));

        public static TValue GetMax<TValue>(this IEnumerable<TValue> collection, Func<TValue, double?> marker)
            => GetExtremum(collection, marker, (a, b) => a.HasValue && (!b.HasValue || a.Value > b.Value));

        public static TValue GetMax<TValue>(this IEnumerable<TValue> collection, Func<TValue, decimal?> marker)
            => GetExtremum(collection, marker, (a, b) => a.HasValue && (!b.HasValue || a.Value > b.Value));

        public static TValue GetMax<TValue>(this IEnumerable<TValue> collection, Func<TValue, sbyte?> marker)
            => GetExtremum(collection, marker, (a, b) => a.HasValue && (!b.HasValue || a.Value > b.Value));

        public static TValue GetMax<TValue>(this IEnumerable<TValue> collection, Func<TValue, byte?> marker)
            => GetExtremum(collection, marker, (a, b) => a.HasValue && (!b.HasValue || a.Value > b.Value));

        public static TValue GetMax<TValue>(this IEnumerable<TValue> collection, Func<TValue, short?> marker)
            => GetExtremum(collection, marker, (a, b) => a.HasValue && (!b.HasValue || a.Value > b.Value));

        public static TValue GetMax<TValue>(this IEnumerable<TValue> collection, Func<TValue, ushort?> marker)
            => GetExtremum(collection, marker, (a, b) => a.HasValue && (!b.HasValue || a.Value > b.Value));

        public static TValue GetMax<TValue>(this IEnumerable<TValue> collection, Func<TValue, int?> marker)
            => GetExtremum(collection, marker, (a, b) => a.HasValue && (!b.HasValue || a.Value > b.Value));

        public static TValue GetMax<TValue>(this IEnumerable<TValue> collection, Func<TValue, uint?> marker)
            => GetExtremum(collection, marker, (a, b) => a.HasValue && (!b.HasValue || a.Value > b.Value));

        public static TValue GetMax<TValue>(this IEnumerable<TValue> collection, Func<TValue, long?> marker)
            => GetExtremum(collection, marker, (a, b) => a.HasValue && (!b.HasValue || a.Value > b.Value));

        public static TValue GetMax<TValue>(this IEnumerable<TValue> collection, Func<TValue, ulong?> marker)
            => GetExtremum(collection, marker, (a, b) => a.HasValue && (!b.HasValue || a.Value > b.Value));

        public static TValue GetMax<TValue>(this IEnumerable<TValue> collection, Func<TValue, nint?> marker)
            => GetExtremum(collection, marker, (a, b) => a.HasValue && (!b.HasValue || a.Value > b.Value));

        public static TValue GetMax<TValue>(this IEnumerable<TValue> collection, Func<TValue, nuint?> marker)
            => GetExtremum(collection, marker, (a, b) => a.HasValue && (!b.HasValue || a.Value > b.Value));


        public static TValue GetExtremum<TValue, TMark>(
            this IEnumerable<TValue> collection,
            Func<TValue, TMark> marker,
            Func<TMark, TMark, bool> comparer)
        {
            var maxElement = default(TValue);

            if (collection.Any())
            {
                using var enumerator = collection.GetEnumerator();

                enumerator.MoveNext();
                maxElement = enumerator.Current;
                var maxValue = marker(maxElement);

                while (enumerator.MoveNext())
                {
                    var value = marker(enumerator.Current);

                    if (comparer(value, maxValue))
                    {
                        maxValue = value;
                        maxElement = enumerator.Current;
                    }
                }
            }

            return maxElement;
        }
    }
}
