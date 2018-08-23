using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BoltFreezer.Utilities
{
    public static class EnumerableExtension
    {
        public static T PickRandom<T>(this IEnumerable<T> source)
        {
            return source.PickRandom(1).Single();
        }

        public static IEnumerable<T> PickRandom<T>(this IEnumerable<T> source, int count)
        {
            return source.Shuffle().Take(count);
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            return source.OrderBy(x => Guid.NewGuid());
        }

        public static List<T> CloneList<T>(List<T> itemsToCopy)
        {
            var newList = new List<T>();
            foreach (var item in itemsToCopy)
            {
                newList.Add(item);
            }
            return newList;
        }

        // From http://community.bartdesmet.net/blogs/bart/archive/2008/11/03/c-4-0-feature-focus-part-3-intermezzo-linq-s-new-zip-operator.aspx
        public static IEnumerable<TResult> Zip<TFirst, TSecond, TResult>(this IEnumerable<TFirst> first, IEnumerable<TSecond> second, Func<TFirst, TSecond, TResult> func)
        {
            if (first == null)
                throw new ArgumentNullException("first");
            if (second == null)
                throw new ArgumentNullException("second");
            if (func == null)
                throw new ArgumentNullException("func");
            using (var ie1 = first.GetEnumerator())
            using (var ie2 = second.GetEnumerator())
                while (ie1.MoveNext() && ie2.MoveNext())
                    yield return func(ie1.Current, ie2.Current);
        }

        public static IEnumerable<KeyValuePair<T, R>> Zip<T, R>(this IEnumerable<T> first, IEnumerable<R> second)
        {
            return first.Zip(second, (f, s) => new KeyValuePair<T, R>(f, s));
        }

        public static List<List<T>> GenerateCombinations<T>(
                                    List<List<T>> collectionOfSeries)
        {
            List<List<T>> generatedCombinations =
                collectionOfSeries.Take(1)
                                  .FirstOrDefault()
                                  .Select(i => (new T[] { i }).ToList())
                                  .ToList();

            foreach (List<T> series in collectionOfSeries.Skip(1))
            {
                generatedCombinations =
                    generatedCombinations
                          .Join(series as List<T>,
                                combination => true,
                                i => true,
                                (combination, i) =>
                                {
                                    List<T> nextLevelCombination =
                                    new List<T>(combination);
                                    nextLevelCombination.Add(i);
                                    return nextLevelCombination;
                                }).ToList();

            }

            return generatedCombinations;
        }

        public static Dictionary<K, V> HashtableToDictionary<K, V>(Hashtable table)
        {
            return table
              .Cast<DictionaryEntry>()
              .ToDictionary(kvp => (K)kvp.Key, kvp => (V)kvp.Value);
        }
    }
}
