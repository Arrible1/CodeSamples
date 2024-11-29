using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ZergRush
{
    public static partial class Utils
    {
        public static TV TryGetOrNew<TK, TV>(this Dictionary<TK, TV> dict, TK key)
            where TV : new()
        {
            TV val;
            if (!dict.TryGetValue(key, out val))
            {
                val = new TV();
                dict[key] = val;
            }
            return val;
        }
        
        public static bool TryFind<T>(this IEnumerable<T> list, Func<T, bool> predicate, out T val)
        {
            val = list.Find(predicate);
            if (val == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        // returns insert position
        public static int InsertSorted<T>(this IList<T> list, Func<T, T, int> predicate, T val)
        {
            var i = list.IndexOf(t => predicate(t, val) >= 0);
            if (i != -1)
            {
                list.Insert(i, val);
                return i;
            }
            else
            {
                list.Add(val);
                return list.Count - 1;
            }
        }

        public static void Check<TK, TV>(this Dictionary<TK, TV> dict, TK key)
            where TV : new()
        {
            if (dict.ContainsKey(key) == false) dict[key] = new TV();
        }

        public static TV AtOrDefault<TK, TV>(this Dictionary<TK, TV> dict, TK key)
            where TV : new()
        {
            TV val;
            if (!dict.TryGetValue(key, out val))
            {
                return default(TV);
            }
            return val;
        }

        public static T Best<T>(this IEnumerable<T> coll, Func<T, float> predicate)
        {
            var best = default(T);
            var curr = Single.MinValue;
            foreach (var v in coll)
            {
                var r = predicate(v);
                if (r > curr)
                {
                    curr = r;
                    best = v;
                }
            }
            return best;
        }

        public static T FirstFilteredOrFirst<T>(this IEnumerable<T> enumerable, Func<T, bool> filter)
        {
            var firstOrDefault = enumerable.FirstOrDefault(filter);
            if (EqualityComparer<T>.Default.Equals(firstOrDefault, default(T)))
            {
                return enumerable.First();
            }
            return firstOrDefault;
        }

        public static void ForeachWithIndices<T>(this IEnumerable<T> value, Action<T, int> act)
        {
            int ix = 0;
            foreach (var e in value)
            {
                act(e, ix);
                ix++;
            }
        }
        public static void ForeachWithIndices<T>(this List<T> value, Action<T, int> act)
        {
            for (var i = 0; i < value.Count; i++)
            {
                act(value[i], i);
            }
        }

        public static int IndexOf<T>(this IEnumerable<T> list, T elem)
        {
            return list.IndexOf(val => EqualityComparer<T>.Default.Equals(val, elem));
        }

        public static int IndexOf<T>(this IEnumerable<T> list, Func<T, bool> predicate)
        {
            int index = 0;
            foreach (var elem in list)
            {
                if (predicate(elem)) return index;
                index++;
            }
            return -1;
        }

        public static IEnumerable<T> Some<T>(params T[] elements)
        {
            return elements;
        }
        
        public static List<T> AddSome<T>(this List<T> list, int count, Func<T> elem)
        {
            for (int i = 0; i < count; ++i) list.Add(elem());
            return list;
        }

        public static List<T> AddSome<T>(this List<T> list, T item)
        {
            list.Add(item);
            return list;
        }

        public static void ZipIterate<T1, T2>(this IEnumerable<T1> coll1, IEnumerable<T2> coll2, Action<T1, T2> func)
        {
            var it1 = coll1.GetEnumerator();
            var it2 = coll2.GetEnumerator();
            while (it1.MoveNext() && it2.MoveNext())
            {
                func(it1.Current, it2.Current);
            }
        } 
        public static void ZipIterate<T1, T2>(this IEnumerable<T1> coll1, IEnumerable<T2> coll2, Action<T1, T2, int> func)
        {
            var it1 = coll1.GetEnumerator();
            var it2 = coll2.GetEnumerator();
            int i = 0;
            while (it1.MoveNext() && it2.MoveNext())
            {
                func(it1.Current, it2.Current, i);
                i++;
            }
        } 

        public static bool AddIfNotContains<T>(this IList<T> list, T item)
        {
            if (list.Contains(item)) return false;
            list.Add(item);
            return true;
        }

        public static bool AddIfNotContainsType<T>(this IList<T> list, T item)
        where T : class
        {
            foreach (var obj in list)
            {
                if (obj.GetType() == item.GetType())
                {
                    return false;
                }
            }
            list.Add(item);
            return true;
        }

        public static IEnumerable<T> Lift<T>(this T self)
        {
            yield return self;
        }

        public static void AddTo<TKey>(this Dictionary<TKey, float> dict, TKey key, float value)
        {
            if (!dict.ContainsKey(key))
            {
                dict[key] = 0;
            }
            dict[key] += value;
        }

        public static TValue GetOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue def)
        {
            return !dict.ContainsKey(key) ? def : dict[key];
        }

        public static int RemoveWhere<TKey, TValue>(this Dictionary<TKey, TValue> dict, Func<TKey, TValue, bool> filter)
        {
            List<TKey> keysToRemove = new List<TKey>();
            foreach (var item in dict)
            {
                if (filter(item.Key, item.Value))
                    keysToRemove.Add(item.Key);
            }
            for (int i = 0; i < keysToRemove.Count; i++)
                dict.Remove(keysToRemove[i]);
            return keysToRemove.Count;
        }

        public static T Take<T>(this List<T> list, int index)
        {
            T t = list[index];
            list.RemoveAt(index);
            return t;
        }
        
        public static TVal TakeKey<TKey, TVal>(this Dictionary<TKey, TVal> dict, TKey key)
        {
            if (dict.TryGetValue(key, out var val))
            {
                dict.Remove(key);
                return val;
            }
            return default;
        }
        
        public static T TakeOne<T>(this List<T> list, Func<T, bool> filter)
        {
            for (var index = 0; index < list.Count; index++)
            {
                var x = list[index];
                if (filter(x))
                {
                    list.RemoveAt(index);
                    return x;
                }
            }
            return default(T);
        }

        public static T TakeLast<T>(this IList<T> list)
        {
            var index = list.Count - 1;
            var t = list[index];
            list.RemoveAt(index);
            return t;
        }
        public static T TakeFirst<T>(this IList<T> list)
        {
            var t = list[0];
            list.RemoveAt(0);
            return t;
        }
        public static void RemoveLast<T>(this IList<T> list)
        {
            list.RemoveAt(list.Count - 1);
        }

        public static T LastElement<T>(this List<T> list, T ifNoElements = default(T))
        {
            return list.Count > 0 ? list[list.Count - 1] : ifNoElements;
        }
        
        // Filter elements of specific type and cast enumerable to that type
        public static IEnumerable<T2> FilterCast<T, T2>(this IEnumerable<T> list)
        {
            return list.Where(e => e is T2).Cast<T2>();
        }
        
        public static string PrintCollection<T>(this IEnumerable<T> collection, string delimiter = ", ")
        {
            if (collection == null) return "empty collection";
            return String.Join(delimiter, collection.Select(val => val?.ToString()).ToArray());
        }

        // Like c++ upper bound. Uses binary search on sorted list
        public static int UpperBound<T>(this List<T> list, T val)
        {
            int index = list.BinarySearch(val);
            if (index >= 0) return index;
            index = ~index;
            return index;
        }

        public static IEnumerable<T> GetEnumValues<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }
        
        public static void EnsureSizeWithNulls<T>(this List<T> list, int count) where T : class
        {
            while (list.Count < count)
            {
                list.Add(null);
            }
        }
        
        
        public static void EnsureSize<T>(this List<T> list, int count) where T : new()
        {
            while (list.Count < count)
            {
                list.Add(new T());
            }
        }
        
        public static void Resize<T>(this List<T> list, int count) where T : new()
        {
            while (list.Count > count)
            {
                list.TakeLast();
            }
            while (list.Count < count)
            {
                list.Add(new T());
            }
        }

        public static void Resize<T>(this List<T> list, int count, Func<T> create, Action<T> destroy)
        {
            while (list.Count > count)
            {
                destroy(list.TakeLast());
            }
            while (list.Count < count)
            {
                list.Add(create());
            }
        }
        
        public static (V, int) MinWithIndex<V>(this IEnumerable<V> list, V baseVal = default)
        {
            int index = 0;
            int currMaxIndex = -1;
            var comparer = Comparer<V>.Default;
            
            foreach (var v in list)
            {
                if (comparer.Compare(v, baseVal) < 0)
                {
                    baseVal = v;
                    currMaxIndex = index;
                }
                index++;
            }
            return (baseVal, currMaxIndex);
        }
        public static (V, int) MaxWithIndex<V>(this IEnumerable<V> list, V baseVal = default)
        {
            int index = 0;
            int currMaxIndex = -1;
            var comparer = Comparer<V>.Default;
            
            foreach (var v in list)
            {
                if (comparer.Compare(v, baseVal) > 0)
                {
                    baseVal = v;
                    currMaxIndex = index;
                }
                index++;
            }
            return (baseVal, currMaxIndex);
        }

        public static (V, int) MaxWithIndex<T, V>(this IEnumerable<T> list, Func<T, V> getVal, V baseVal = default)
        {
            return list.Select(getVal).MaxWithIndex(baseVal);
        }
        
        public static (V, int) MinWithIndex<T, V>(this IEnumerable<T> list, Func<T, V> getVal, V baseVal = default)
        {
            return list.Select(getVal).MinWithIndex(baseVal);
        }
        
        
        public static string WithoutSuffix(this string str, string suffix)
        {
            var suff = str.IndexOf(suffix, StringComparison.OrdinalIgnoreCase);
            if (suff == -1) return str;
            return str.Substring(0, suff);
        }

        public static void SaveToBinaryFile<T>(T data, string path) where T : ISerializable
        {
            try
            {
                Debug.Log($"Saving {data} to : " + path);
                using (var file = FileWrapper.Open(path, FileMode.Create))
                {
                    data.Serialize(new BinaryWriter(file));
                    file.Flush();
                    file.Close();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save data to path {data} with error :");
                Debug.LogError(e.Message);
            }
        }
        
        public static bool LoadFromBinaryFile<T>(string path, out T data) where T : ISerializable, new()
        {
            data = new T();
            try
            {
                using (var file = FileWrapper.Open(path, FileMode.Open))
                {
                    data.Deserialize(new BinaryReader(file));
                    if (data is ILivable livable) livable.Enlive();
                }

                return true;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log($"Failed to load {typeof(T)} from path: " + path);
                return false;
            }
        }

        public static void SaveToJsonFile<T>(this T data, string path, bool formatting = true)
            where T : IJsonSerializable
        {
            try
            {
                UnityEngine.Debug.Log($"Saving {data} to : " + path);
                using (var file = FileWrapper.CreateText(path))
                {
                    var writer = new JsonTextWriter(file);
                    writer.Formatting = formatting ? Formatting.Indented : Formatting.None;
                    data.WriteJson(writer);
                    file.Flush();
                    file.Close();
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(
                    $"Failed to save data to path {data} with error :{e.Message} stacktrace =\n{e.StackTrace}");
            }
        }

        public static bool LoadFromJsonFile<T>(string path, out T data, bool printError = true)
            where T : class, IJsonSerializable, new()
        {
            try
            {
                using (var file = FileWrapper.OpenText(path))
                {
                    data = new T();
                    data.ReadFromJson(new JsonTextReader(file));
                    if (data is ILivable livable) livable.Enlive();
                    return true;
                }
            }
            catch (Exception e)
            {
                if (printError)
                {
                    Debug.Log($"Failed to load {typeof(T)} from file: " + path);
                    Debug.Log(e.ToString());
                }

                data = null;
                return false;
            }
        }

        public static string LayerSpeedParamName(this Animator animator, int layer) =>
            "Speed" + animator.GetLayerName(layer);

        public static void SetDefaultPosition(this Transform transform)
        {
            transform.localPosition = UnityEngine.Vector3.zero;
            transform.localRotation = UnityEngine.Quaternion.identity;
            transform.localScale = UnityEngine.Vector3.one;
        }

        public static void DestroyChildren(this Transform t, Func<Transform, bool> predicate)
        {
            foreach (var child in t)
            {
                var c = (Transform) child;
                if (predicate(c))
                    Object.Destroy(c.gameObject);
            }
        }

        public static byte[] SaveToBinary<T>(this T data) where T : ISerializable 
        {
            try
            {
                using (var stream = new MemoryStream())
                {
                    var writer = new BinaryWriter(stream);
                    data.Serialize(writer);
                    return stream.ToArray();
                }
            }
            catch (Exception e)
            {
                ServerEngine.Debug.LogError($"Failed to save data to bytes {data} with error :");
                ServerEngine.Debug.LogError(e.Message + e.StackTrace);
                throw;
            }
        }

        public static List<int> SelectRandomIndsFromWeights(this List<float> weights, int maxCount)
        {
            List<int> selectedInds = new List<int>();
            while (selectedInds.Count < maxCount && weights.Count > selectedInds.Count)
            {
                // Sum all not selectedTypeName weights.
                float sum = 0;
                for (int i = 0; i < weights.Count; i++)
                {
                    if (selectedInds.Contains(i))
                        continue;
                    sum += weights[i];
                }
                // Find next random ind.
                float rand = UnityEngine.Random.value * sum;
                int selectedInd = -1;
                for (int i = 0; i < weights.Count; i++)
                {
                    if (selectedInds.Contains(i))
                        continue;
                    rand -= weights[i];
                    if (rand <= 0)
                    {
                        selectedInd = i;
                        break;
                    }
                }
                if (selectedInd == -1)
                    throw new Exception("WTF");
                selectedInds.Add(selectedInd);
            }
            return selectedInds;
        }
    }
}