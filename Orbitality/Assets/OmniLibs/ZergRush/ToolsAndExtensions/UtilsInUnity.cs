using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;
using ZergRush.ReactiveCore;
using Debug = ServerEngine.Debug;
using Object = UnityEngine.Object;

namespace ZergRush
{
    public static partial class Utils {
        
        public static IEnumerable<T> AllValues<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }

        public static void SafeAdd(this Cell<int> cell, int val)
        {
            if (cell.value > 0) cell.value -= val;
        }

        public static void SafeSubstract(this Cell<int> cell, int val, int maxVal)
        {
            if (cell.value < maxVal) cell.value += val;
        }
        public static void SetTransparent(this Image image, bool transparent)
        {
            image.color = image.color.WithAlpha(transparent ? 0 : 1);
        }
        public static string ToError(this Exception e)
        {
            return $"{e.Message}\n{e.StackTrace}";
        }
        public static void SaveToFile(this ISerializable obj, string filePath, bool wrapfile)
        {
            using (BinaryWriter writer = new BinaryWriter(OpenFileWrap(filePath, FileMode.Create, wrapfile)))
            {
                obj.Serialize(writer);
            }
        }
        public static T ReadFromFile<T>(string filePath, bool wrapPath, T instance = null) where T : class, ISerializable, new()
        {
            using (BinaryReader reader = new BinaryReader(OpenFileWrap(filePath, FileMode.Open, wrapPath)))
            {
                if (instance == null)
                    instance = new T();
                instance.Deserialize(reader);
                return instance;
            }
        }
        public static void SaveToFile(this byte[] obj, string filePath, bool wrapPath)
        {
            using (BinaryWriter writer = new BinaryWriter(OpenFileWrap(filePath, FileMode.Create, wrapPath)))
            {
                writer.WriteByteArray(obj);
            }
        }
        public static void SaveToFilePure(this byte[] obj, string filePath, bool wrapPath)
        {
            using (BinaryWriter writer = new BinaryWriter(OpenFileWrap(filePath, FileMode.Create, wrapPath)))
            {
                writer.Write(obj);
            }
        }
        static FileStream OpenFileWrap(string path, FileMode mode, bool wrap)
        {
            return wrap ? FileWrapper.Open(path, mode) : File.Open(path, mode);
        }
        public static string TryReadAllText(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                    return File.ReadAllText(filePath);
                else
                    return String.Empty;
            }
            catch
            {
                return String.Empty;
            }
        }

        
        public static bool Contains<T>(this T[] list, T t)
        {
            for (var i = 0; i < list.Length; i++)
            {
                var x1 = list[i];
                if (EqualityComparer<T>.Default.Equals(t, x1)) return true;
            }
            return false;
        }

        public static void Swap<T>(ref T item1, ref T item2)
        {
            T temp = item1;
            item1 = item2;
            item2 = temp;
        }

        public static bool RemoveOne<T>(this List<T> list, Predicate<T> whatToRemove)
        {
            for (int i = 0; i < list.Count; i++)
            {
                bool found = whatToRemove(list[i]);
                if (!found)
                    continue;
                list.RemoveAt(i);
                return true;
            }
            return false;
        }
        public static void RemoveAll<T>(this IList<T> list, Predicate<T> whatToRemove)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                bool found = whatToRemove(list[i]);
                if (found)
                    list.RemoveAt(i);
            }
        }
        public static void InsertSorted<T, C>(this List<T> list, T item, Func<T, C> whatToCompare) where C : IComparable
        {
            C itemValue = whatToCompare(item);
            int minInd = 0;
            int maxInd = list.Count;
            while (minInd < maxInd)
            {
                int middleInd = (minInd + maxInd) / 2;
                C middleValue = whatToCompare(list[middleInd]);
                int compare = itemValue.CompareTo(middleValue);
                if (compare < 0)
                    maxInd = middleInd;
                else if (compare > 0)
                    minInd = middleInd + 1;
                else
                {
                    minInd = middleInd;
                    maxInd = middleInd;
                }
            }
            int insertInd = minInd;
            list.Insert(insertInd, item);
        }
        public static T[] WithoutInd<T>(this T[] array, int ind)
        {
            T[] res = new T[array.Length - 1];
            for (int i = 0; i < res.Length; i++)
            {
                int j = i;
                if (i >= ind)
                    j++;
                res[i] = array[j];
            }
            return res;
        }
        public static T[] With<T>(this T[] array, T newItem)
        {
            T[] res = new T[array.Length + 1];
            for (int i = 0; i < array.Length; i++)
                res[i] = array[i];
            res[array.Length] = newItem;
            return res;
        }

        public static T Min<T>(this IEnumerable<T> collection) where T : IComparable
        {
            T min = collection.FirstOrDefault();
            foreach (var item in collection)
            {
                if (item.CompareTo(min) < 0)
                    min = item;
            }
            return min;
        }
        public static T Max<T>(this IEnumerable<T> collection) where T : IComparable
        {
            T max = collection.FirstOrDefault();
            foreach (var item in collection)
            {
                if (item.CompareTo(max) > 0)
                    max = item;
            }
            return max;
        }
        public static List<T> Filter<T>(this List<T> items, Func<T, bool> filter)
        {
            List<T> filtered = new List<T>();
            foreach (var item in items)
            {
                if (filter(item))
                    filtered.Add(item);
            }
            return filtered;
        }
        public static List<T> Filter<T>(this T[] items, Func<T, bool> filter)
        {
            List<T> filtered = new List<T>();
            foreach (var item in items)
            {
                if (filter(item))
                    filtered.Add(item);
            }
            return filtered;
        }
        public static long AddToHash(this long hash, int val)
        {
            hash += (uint)val;
            hash += hash << 11; hash ^= hash >> 7;
            return hash;
        }
        public static List<To> ConvertAll<From, To>(this IEnumerable<From> array, Func<From, To> convert)
        {
            List<To> res = new List<To>();
            foreach (var item in array)
                res.Add(convert(item));
            return res;
        }
        public static List<To> ConvertAll<From, To>(this From[] array, Func<From, To> convert)
        {
            List<To> res = new List<To>();
            for (int i = 0; i < array.Length; i++)
                res.Add(convert(array[i]));
            return res;
        }
        public static List<To> ConvertAll<To>(this Array array, Func<object, To> convert)
        {
            List<To> res = new List<To>();
            foreach (var item in array)
                res.Add(convert(item));
            return res;
        }
        public static void ForEach<T>(this T[] array, Action<T> action)
        {
            if (array == null) return;
            for (int i = 0; i < array.Length; i++)
                action(array[i]);
        }
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (action == null) throw new ArgumentNullException(nameof(action));

            foreach (var item in source)
                action(item);
        }
        public static void ForEach<T>(this IList<T> list, Action<T> action)
        {
            if (list == null) return;
            for (int i = 0; i < list.Count; i++)
                action(list[i]);
        }
        public static void ForEach<TKey, TValue>(this Dictionary<TKey, TValue>.ValueCollection list, Action<TValue> action)
        {
            if (list == null) return;
            foreach (var item in list)
                action(item);
        }
        public static T Find<T>(this IEnumerable<T> array, Func<T, bool> predicate)
        {
            if (array == null)
                return default(T);
            foreach (var item in array)
            {
                if (predicate(item))
                    return item;
            }
            return default(T);
        }
        public static List<T> FindAll<T>(this T[] array, Func<T, bool> predicate)
        {
            List<T> res = new List<T>();
            for (int i = 0; i < array.Length; i++)
            {
                if (predicate(array[i]))
                    res.Add(array[i]);
            }
            return res;
        }
        
        public static double GetDouble(string value, double defaultValue)
        {
            double result;

            if (!Double.TryParse(value, NumberStyles.Any, CultureInfo.CurrentCulture, out result) &&
                !Double.TryParse(value, NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"), out result) &&
                !Double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
            {
                result = defaultValue;
            }
            return result;
        }
                
        
        public static byte[] ReadFromFile(string filePath)
        {
            if (!File.Exists(filePath))
                return null;
            using (BinaryReader reader = new BinaryReader(File.Open(filePath, FileMode.Open)))
            {
                byte[] bytes = reader.ReadByteArray();
                return bytes;
            }
        }

        public static T DeserializeSource<T>(byte[] data, T instance = null) where T : class, ISerializable, new()
        {
            instance = DeserializeFromBytes(data, instance);
            return instance;
        }

        public static T DeserializeFromBytes<T>(byte[] bytes, T instance = null) where T : class, ISerializable, new()
        {////////////////
            if (instance == null) instance = new T();
            using (BinaryReader reader = new BinaryReader(new MemoryStream(bytes)))
            {
                instance.Deserialize(reader);
            }
            return instance;
        }
        public static byte[] SerializeToBytes(this ISerializable val)
        {
            using (MemoryStream memStream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(memStream))
                {
                    val.Serialize(writer);
                }
                return memStream.ToArray();
            }
        }

        public static T ReadFromResourcesSerializable<T>(string filePath, T instance = null)
            where T : class, ISerializable, new()
        {
            var asset = Resources.Load<TextAsset>(filePath);
            instance = DeserializeFromBytes(asset.bytes, instance);
            return instance;
        }

        public static string EncodeToString(this ISerializable val)
        {
            byte[] encodedBytes = val.SerializeToBytes();
            string encodedString = Convert.ToBase64String(encodedBytes);
            return encodedString;
        }
        public static T DecodeFromString<T>(string encodedString) where T : class, ISerializable, new()
        {
            byte[] bytesEncoded = Convert.FromBase64String(encodedString);
            T val = DeserializeFromBytes<T>(bytesEncoded);
            return val;
        }

        // Value is difference bhetween current and next value.
        public static IDisposable BindDiff(this ICell<float> cell, Action<float> action)
        {
            // Implicit lambda boxing used as a prev val storage here
            float prevVal = cell.value;
            return cell.Bind(v =>
            {
                action(v - prevVal);
                prevVal = v;
            });
        }

        // Value is difference bhetween current and next value.
        public static ICell<float> Diff(this ICell<float> cell)
        {
            // Implicit lambda boxing used as a prev val storage here
            float prevVal = cell.value;
            return new AnonymousCell<float>(action =>
            {
                return cell.Bind(v =>
                {
                    action(v - prevVal);
                    prevVal = v;
                });
            }, () => cell.value - prevVal);
        }

        public static IEnumerable<T> Yield<T>(this T t)
        {
            yield return t;
        }
        public static void AddIfNotNull<T>(this List<T> list, T t) where T : class
        {
            if (t != null) list.Add(t);
        }

        public static bool ParseBool(this string str)
        {
            return str.ToLower() == "true";
        }

        public static T LoadFromJsonFile<T>(string path, Func<T> defaultData = null, bool printError = true)
            where T : class, IJsonSerializable, new()
        {
            T data;
            if (!LoadFromJsonFile(path, out data, printError))
                return defaultData != null ? defaultData.Invoke() : new T();
            else
                return data;
        }

        public static bool LoadFromJsonFileIfExists<T>(string path, out T data, bool printError = true)
            where T : class, IJsonSerializable, new()
        {
            if (!FileWrapper.Exists(path))
            {
                data = default;
                return false;
            }
            else
                return LoadFromJsonFile(path, out data, printError);
        }

        public static void SaveJsonToFileRemoveOnNull<T>(this T data, string path, bool formatting = true)
            where T : class, IJsonSerializable
        {
            if (data == null)
                FileWrapper.RemoveIfExists(path);
            else
                data.SaveToJsonFile(path, formatting);
        }

        public static void SetAlpha(this Image img, float alpha)
        {
            img.color = new Color(img.color.r, img.color.g, img.color.b, alpha);
        }

        public static void DestroyChildren(this Transform t)
        {
            foreach (var child in t)
            {
                Object.Destroy(((Transform) child).gameObject);
            }
        }

        public static string SaveToJsonString<T>(this T data, bool formatting = true) where T : IJsonSerializable 
        {
            try
            {
                using (var stream = new StringWriter())
                {
                    var writer = new JsonTextWriter(stream);
                    writer.Formatting = formatting ? Formatting.Indented : Formatting.None;
                    data.WriteJson(writer);
                    return stream.ToString();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save data to path {data} with error :");
                Debug.LogError(e.Message + e.StackTrace);
                throw;
            }
        }

        public static T LoadFromJsonString<T>(this string content) where T : IJsonSerializable, new()
        {
            var data = new T();
            try
            {
                using (var reader = new StringReader(content))
                {
                    data.ReadFromJson(new JsonTextReader(reader));
                }
            }
            catch (Exception e)
            {
                data = new T();
                Debug.LogError($"Failed to load json from string {content} with error :{e.Message} call stack=\n{e.StackTrace}");
            }

            return data;
        }
    }
}
