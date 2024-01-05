using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResoniteWikiHelpers.ProtofluxNodeExport
{
    internal static class EnumerableExtensions
    {
        public static IEnumerable<T> Interleave<T>(this IEnumerable<T> first, IEnumerable<T> second)
        {
            var firstEnumerator = first.GetEnumerator();
            var secondEnumerator = second.GetEnumerator();

            bool firstNext;
            bool secondNext;

            while ((firstNext = firstEnumerator.MoveNext()) | (secondNext = secondEnumerator.MoveNext()))
            {
                if (firstNext)
                    yield return firstEnumerator.Current;

                if (secondNext)
                    yield return secondEnumerator.Current;
            }
        }
    }
}