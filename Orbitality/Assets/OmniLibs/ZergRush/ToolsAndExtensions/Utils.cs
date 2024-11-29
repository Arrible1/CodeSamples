using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using ZergRush.ReactiveCore;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TWL;
using UnityEngine;
using Debug = ServerEngine.Debug;
using Frame = ServerEngine.Frame;

namespace ZergRush
{
    public static partial class Utils
    {
        public static int Clamp(this int value, int min, int max)
        {
            return Math.Min(Math.Max(value, min), max);
        }

        public static async Task ForEachAsync<T>(this IEnumerable<T> items, Func<T, Task> action)
        {
            foreach (var item in items)
                await action(item);
        }

        public static T GetAttribute<T>(this Type type) where T : Attribute
        {
            return (T) type.GetCustomAttributes(typeof(T), false)[0];
        }

        public static bool IsNullOrEmpty(this string str)
        {
            return String.IsNullOrEmpty(str);
        }

        public static bool Valid(this string str)
        {
            return !String.IsNullOrEmpty(str);
        }

        public static async Task DoAsyncWithMaxSimultaneous(IEnumerable<Func<Task>> tasks, int maxSimultaneous)
        {
            Debug.Assert(maxSimultaneous >= 1, "should allow at least one task at a time");
            List<Task> currTasks = new List<Task>();
            foreach (var task in tasks)
            {
                // Wait until simultaneous tasks count drops below limit.
                await WaitingUntillTasksCountLessThan(maxSimultaneous);
                // Start next task.
                currTasks.Add(task());
            }

            // Wait until all tasks complete.
            await WaitingUntillTasksCountLessThan(1);

            async Task WaitingUntillTasksCountLessThan(int count)
            {
                while (currTasks.Count >= count)
                {
                    await Frame.one;
                    currTasks.RemoveAll(currTask => currTask.IsCompleted);
                }
            }
        }

        public static void Deconstruct<T, U>(this KeyValuePair<T, U> k, out T t, out U u)
        {
            t = k.Key;
            u = k.Value;
        }

        public static string FormatEach<T>(this IEnumerable<T> self, string format)
        {
            return FormatEach(self, format, arg1 => arg1);
        }

        public static string FormatEach<T>(this IEnumerable<T> self)
        {
            return FormatEach(self, "{0}", arg1 => arg1);
        }

        public static string FormatEach<T>(this IEnumerable<T> self, Func<T, object> parameter)
        {
            return FormatEach(self, "{0}", parameter);
        }

        public static string FormatEach<T>(this IEnumerable<T> self, string format, params Func<T, object>[] parameters)
        {
            var builder = new StringBuilder();

            foreach (var value in self)
            {
                builder.AppendFormat(format + "\n", parameters.Select(func => func(value)).ToArray());
            }

            return builder.ToString();
        }
        
        public static string UpperFirstLetter(this string str)
        {
            if (str == null)
                return null;

            if (str.Length > 1)
                return char.ToUpper(str[0]) + str.Substring(1);

            return str.ToUpper();
        }


        public static string TextToCamelCase(this string str)
        {
            var builder = new StringBuilder();
            foreach (var s in str.Split(' '))
            {
                builder.Append(s.UpperFirstLetter());
            }

            return builder.ToString();
        }

        public static string CamelCaseToReadableText(this string name)
        {
            return Regex.Replace(name, "([a-z](?=[A-Z])|[A-Z](?=[A-Z][a-z]))", "$1 ");
        }

        public static string CamelCaseToUnderscored(this string name)
        {
            return Regex.Replace(name, "([a-z](?=[A-Z])|[A-Z](?=[A-Z][a-z]))", "$1_");
        }

        public static T CastTo<T>(this string str) where T : new()
        {
            if (str.Length > 0)
            {
                if (str.Contains("%"))
                {
                    str = str.Replace("%", "");
                    double tempDouble = Convert.ToDouble(str) / 100.0;
                    str = tempDouble.ToString();
                }

                try
                {
                    return (T) Convert.ChangeType(str, typeof(T));
                }
                catch (Exception)
                {
                    //Debug.LogError(e.Message + " : " + str + "; " + e.StackTrace);
                    return new T();
                }
            }

            return default(T);
        }

        public static string FormatGameNumber(float number)
        {
            if (number > 10) return number.ToString("0.");
            else if (number > 1) return number.ToString("0.0");
            else return number.ToString("0.00");
        }

        public class Wrapper<T>
        {
            public T value;
        }

        public static Wrapper<T> Wrap<T>(T value)
        {
            return new Wrapper<T> {value = value};
        }

        public static int Loop(int i, int cycle)
        {
            int r = i % cycle;
            if (r < 0) r += cycle;
            return r;
        }

        public static bool HasAttribute<T>(this MemberInfo field) where T : Attribute
        {
            return Attribute.IsDefined(field, typeof(T));
        }

        public static T Clone<T>(this T source) where T : class
        {
            if (!typeof(T).IsSerializable)
            {
                throw new ArgumentException("The type must be serializable.", "source");
            }

            // Don't serialize a null object, simply return the default for that object
            if (ReferenceEquals(source, null))
            {
                return default(T);
            }

            IFormatter formatter = new BinaryFormatter();
            Stream stream = new MemoryStream();
            using (stream)
            {
                formatter.Serialize(stream, source);
                stream.Seek(0, SeekOrigin.Begin);
                return (T) formatter.Deserialize(stream);
            }
        }

        public static void InvokeSafe(this Action action)
        {
            if (action != null) action();
        }


        public static void SafeAdd(this Cell<uint> cell, int val, int max)
        {
            cell.value = (uint) Math.Min(max, cell.value + val);
        }

        public static void SafeSubstract(this Cell<uint> cell, int val)
        {
            cell.value = (uint) Math.Max(0, cell.value - val);
        }

        public static void SafeSubstract(this Cell<float> cell, float val)
        {
            cell.value = Math.Max(0, cell.value - val);
        }

        public static void SafeSubstract(this Cell<ushort> cell, int val)
        {
            cell.value = (ushort) Math.Max(0, cell.value - val);
        }

        public static void SafeSubstract(this Cell<byte> cell, int val)
        {
            cell.value = (byte) Math.Max(0, cell.value - val);
        }

        public static void AddLooped(this Cell<float> cell, float val, float loop)
        {
            cell.value += val;
            if (cell.value >= loop) cell.value -= loop;
        }

        public static List<T> GetReversed<T>(this IEnumerable<T> list)
        {
            List<T> res = new List<T>();
            foreach (var item in list)
                res.Insert(0, item);
            return res;
        }

        public static List<T> DifferenceNoLINQ<T>(this IEnumerable<T> list, IEnumerable<T> except)
        {
            var res = new List<T>();

            var intercepted = false;
            foreach (var item in list)
            {
                intercepted = false;
                foreach (var item2 in except)
                {
                    if (EqualityComparer<T>.Default.Equals(item, item2))
                    {
                        intercepted = true;
                        break;
                    }
                }

                if (!intercepted)
                    res.Add(item);
            }

            return res;
        }

        public static IDisposable ShowWhile(this MonoBehaviour beh, ICell<bool> val)
        {
            return beh.SetActive(val);
        }

        public static void SetLayerSpeed(this Animator animator, int layer, float speed)
        {
            animator.SetFloat(animator.LayerSpeedParamName(layer), speed);
        }

        public static Transform FindRecursive(this Transform tr, string name)
        {
            Transform found = tr.Find(name);
            if (found != null)
                return found;
            for (int i = 0; i < tr.childCount; i++)
            {
                found = FindRecursive(tr.GetChild(i), name);
                if (found != null)
                    return found;
            }

            return null;
        }

        public static void CheckSerialization<T>(T c) where T : class, IJsonSerializable, ICompareChechable<T>, new()
        {
            var str = new StringWriter();
            var writer = new JsonTextWriter(str);
            c.WriteJson(writer);
            var result = str.ToString();

            var reader = new JsonTextReader(new StringReader(result));
            var c2 = reader.ReadAsJsonRoot<T>();

            c.CompareCheck(c2, new Stack<string>());
        }

        public static T LoadFromBinary<T>(this byte[] content, Func<T> defaultIfLoadFailed = null)
            where T : ISerializable, new()
        {
            var data = new T();
            try
            {
                using (var reader = new MemoryStream(content))
                {
                    data.Deserialize(new BinaryReader(reader));
                }
            }
            catch (Exception e)
            {
                if (defaultIfLoadFailed != null)
                    data = defaultIfLoadFailed();
                else
                    data = new T();
                Debug.LogError($"Failed to load binary with error :{e.Message} call stack=\n{e.StackTrace}");
            }

            return data;
        }
    }
}