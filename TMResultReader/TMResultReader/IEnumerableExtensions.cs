using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;

namespace TMResultReader
{
    internal static class IEnumerableExtensions
    {
        public static IEnumerable<TSource> MaxElements<TSource, TComparable>(this IEnumerable<TSource> source, Func<TSource, TComparable> selector)
            where TComparable : IComparable<TComparable>
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(selector != null);
            Contract.Ensures(Contract.Result<IEnumerable<TSource>>() != null);

            List<TSource> results = new List<TSource>();

            TComparable maxValue = default(TComparable);
            foreach (TSource s in source) {
                TComparable e = selector(s);

                var comp = maxValue.CompareTo(e);
                if (comp <= 0) {
                    maxValue = e;
                    if (comp != 0) {
                        results.Clear();
                    }
                    results.Add(s);
                }
            }

            return results;
        }
    }
}
