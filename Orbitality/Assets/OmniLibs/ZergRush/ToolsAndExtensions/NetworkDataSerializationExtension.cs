using System;
using System.Collections.Generic;
using System.IO;
using ZergRush;

namespace ZeroLag
{
    static class NetworkDataSerializationExtension
    {
        public static T Deserialize<T>(this byte[] bytes, int bytesCount, int ctorArgsCount) where T : ISerializable
        {
            using (var memoryStream = new MemoryStream())
            {
                memoryStream.Write(bytes, 0, bytesCount);
                memoryStream.Seek(0, SeekOrigin.Begin);

                using (var sr = new BinaryReader(memoryStream))
                {
                    T command = CreateInstanceFast<T>(ctorArgsCount);
                    command.Deserialize(sr);
                    return command;
                }
            }
        }
        public static object Deserialize(this byte[] bytes, int bytesCount, int ctorArgsCount, Type type)
        {
            using (var memoryStream = new MemoryStream())
            {
                memoryStream.Write(bytes, 0, bytesCount);
                memoryStream.Seek(0, SeekOrigin.Begin);

                using (var sr = new BinaryReader(memoryStream))
                {
                    var command = (ISerializable)CreateInstanceFast(ctorArgsCount, type);
                    command.Deserialize(sr);
                    return command;
                }
            }
        }
        /// <summary>
        /// Slower than if you give ctor args count.
        /// </summary>
        public static T Deserialize<T>(this byte[] bytes, int bytesCount) where T : ISerializable
        {
            using (var memoryStream = new MemoryStream())
            {
                memoryStream.Write(bytes, 0, bytesCount);
                memoryStream.Seek(0, SeekOrigin.Begin);

                using (var sr = new BinaryReader(memoryStream))
                {
                    T command;
                    int ctorArgsCount;
                    CreateInstance<T>(out command, out ctorArgsCount);
                    command.Deserialize(sr);
                    return command;
                }
            }
        }
        public static byte[] Serialize(this ISerializable command, int bufferSize)
        {
            using (var stream = new MemoryStream(bufferSize))
            {
                using (var sw = new BinaryWriter(stream))
                {
                    command.Serialize(sw);
                    sw.Flush();
                }
                return stream.ToArray();
            }
        }
        private static List<object[]> ctorArgsByCount = new List<object[]>();
        public static void CreateInstance<T>(out T instance, out int ctorArgsCount) where T : ISerializable
        {
            ctorArgsCount = 0;
            while (true)
            {
                try
                {
                    object[] argsWithCurrCount = null;
                    if (ctorArgsByCount.Count<=ctorArgsCount)
                        ctorArgsByCount.Add(new object[ctorArgsCount]);
                    argsWithCurrCount = ctorArgsByCount[ctorArgsCount];
                    instance = (T)Activator.CreateInstance(typeof(T), argsWithCurrCount);
                    return;
                }
                catch
                {
                    ctorArgsCount++;
                }
            }
        }
        public static T CreateInstanceFast<T>(int argsCount) where T : ISerializable
        {
            return (T)CreateInstanceFast(argsCount, typeof(T));
        }
        public static object CreateInstanceFast(int argsCount, Type t)
        {
            if (argsCount==0)
                return Activator.CreateInstance(t);
            return Activator.CreateInstance(t, ctorArgsByCount[argsCount]);
        }

        public static List<T> ReadList<T>(this BinaryReader br, Func<T> readElement)
        {
            int count = br.ReadInt32();
            List<T> items = new List<T>();
            for (int i = 0; i < count; i++)
                items.Add(readElement());
            return items;
        }
        public static void WriteList<T>(this BinaryWriter bw, int count, Action<int> writeElement)
        {
            bw.Write(count);
            for (int i = 0; i < count; i++)
                writeElement(i);
        }

        public static T[] ReadArray<T>(this BinaryReader br, Func<T> readElement)
        {
            int count = br.ReadInt32();
            T[] items = new T[count];
            for (int i = 0; i < count; i++)
                items[i] = readElement();
            return items;
        }
        public static void WriteArray<T>(this BinaryWriter bw, T[] array, Action<T> writeElement)
        {
            bw.Write(array.Length);
            for (int i = 0; i < array.Length; i++)
                writeElement(array[i]);
        }
    }
}