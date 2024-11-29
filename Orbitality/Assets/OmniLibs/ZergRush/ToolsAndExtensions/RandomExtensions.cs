using System.Collections.Generic;
using System.Linq;
using TWL;

namespace ZergRush
{
    public static partial class RandomExtensions
    {
        public static List<float> NormalizeFloatRange(this IEnumerable<float> range)
        {
            var list = range.ToList();
            var magnitude = list.Sum();
            if (magnitude == 0) return list;
            for (int i = 0; i < list.Count; i++)
            {
                list[i] /= magnitude;
            }
            return list;
        }

        public static IEnumerable<T> RandomElements<T>(this List<T> list, ZergRandom random, int count)
        {
            return RandomNonoverlappedIndices(list.Count, count, random).Select(i => list[i]);
        }
        
        public static T Enum<T>(this ZergRandom random)
        {
            // fuck c#
            var vals = System.Enum.GetValues(typeof(T));
            return (T)vals.GetValue(random.Range(0, vals.Length));
        }

        public static List<T> Shuffle<T>(this IReadOnlyList<T> list, ZergRandom random) => list.RandomOrder(random);
        public static List<T> RandomOrder<T>(this IReadOnlyList<T> list, ZergRandom random)
        {
            var indexes = RandomNonoverlappedIndices(list.Count, list.Count, random);
            var result = new List<T>(list.Count);
            for (int i = 0; i < list.Count; i++) { result.Add(default(T)); }
            for (var i = 0; i < indexes.Length; i++)
            {
                var index = indexes[i];
                result[i] = list[index];
            }
            return result;
        }
        
        public static T RandomElement<T>(this IEnumerable<T> list, ZergRandom random)
        {
            return list.ToList().RandomElement(random);
        }

        public static T RandomElement<T>(this ICollection<T> list, ZergRandom random)
        {
            if (list.Count < 1)
                return default(T);

            return list.ElementAt(random.Next(0, list.Count));
        }
        
        // max is exclusive index max, count is number of random indexes returned
        public static int[] RandomNonoverlappedIndices(int max, int count, ZergRandom random)
        {
            if (max < count)
            {
                int[] r = new int[max];
                for (int i = 0; i < max; i++)
                {
                    r[i] = i;
                }
                return r;
            }
            int[] result = new int[count];
            var range = Enumerable.Range(0, max).ToList();
            for (int i = 0; i < count; ++i)
            {
                int randIndex = random.Next(0, max - i);
                int rand = range[randIndex];
                result[i] = rand;
                range[randIndex] = range[max - i - 1];
            }

            return result;
        }

        public static bool ChancePercent(this ZergRandom rand, int percent)
        {
            return rand.NextDouble() * 100 < percent;
        }
        
        public static bool ChancePercent(this ZergRandom rand, float percent)
        {
            return rand.NextDouble() * 100 < percent;
        }
        
        public static bool Chance(this ZergRandom rand, float chance)
        {
            return rand.NextDouble() * 100 < chance;
        }
        
        public static int Range(this ZergRandom rand, int min, int max)
        {
            if (max <= min) return min;
            return min + rand.Next() % (max - min);
        }
        public static float Range(this ZergRandom rand, float min, float max)
        {
            return (float)rand.NextDouble() * (max - min) + min;
        }
    }
}