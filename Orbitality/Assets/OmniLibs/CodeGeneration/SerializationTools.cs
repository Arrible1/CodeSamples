using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ZergRush.Alive;
using Newtonsoft.Json;
using ZergRush;
using UnityEngine;
using Debug = ServerEngine.Debug;

public class JsonSerializationException : Exception
{
    public JsonSerializationException(string message) : base(message)
    {
    }
}

public static class SerializationTools
{
    public static T ReadSerializable<T>(this BinaryReader r) where T : ISerializable, new()
    {
        var val = new T();
        val.Deserialize(r);
        return val;
    }

    public static void Write(this BinaryWriter r, ISerializable data)
    {
        data.Serialize(r);
    }

    public static string ToBase64(this byte[] bytes)
    {
        return Convert.ToBase64String(bytes);
    }
    
    public static byte[] FromBase64(this string str)
    {
        return Convert.FromBase64String(str);
    }

    public static int ParsePercent(this string str)
    {
        if (string.IsNullOrEmpty(str)) return 0;
        str = str.Replace("%", "");
        return int.Parse(str);
    }
    public static int ParsePartToPercent(this string str)
    {
        if (string.IsNullOrEmpty(str)) return 0;
        return Mathf.RoundToInt(float.Parse(str) * 100);
    }
    
    public static int ParseInt(this string str)
    {
        if (string.IsNullOrEmpty(str)) return 0;
        return int.Parse(str);
    }

    public static IEnumerable<TEnum> EnumValues<TEnum>()
    {
        return Enum.GetValues(typeof(TEnum)).Cast<TEnum>();
    }
    
    public static TEnum ParseEnum<TEnum>(this string str) where TEnum : struct
    {
        TEnum val;
        if (Enum.TryParse(str.TextToCamelCase(), true, out val) == false)
        {
            return default(TEnum);
        }

        return val;
    }

    static Dictionary<Type, Func<BinaryReader, object>> readers = new Dictionary<Type, Func<BinaryReader, object>>()
    {
        {typeof(byte), reader => reader.ReadByte()},
        {typeof(ushort), reader => reader.ReadUInt16()},
        {typeof(short), reader => reader.ReadInt16()},
        {typeof(uint), reader => reader.ReadUInt32()},
        {typeof(int), reader => reader.ReadInt32()},
        {typeof(ulong), reader => reader.ReadUInt64()},
        {typeof(long), reader => reader.ReadInt64()},
    };

    public static T ReadEnum<T>(this BinaryReader stream)
    {
        Type t = Enum.GetUnderlyingType(typeof(T));
        object val = readers[t](stream);
        return (T) val;
    }

    public static byte[] ReadByteArray(this BinaryReader stream)
    {
        int size = stream.ReadInt32();
        return stream.ReadBytes(size);
    }

    public static void WriteByteArray(this BinaryWriter stream, byte[] bytes)
    {
        int size = bytes.Length;
        stream.Write(size);
        stream.Write(bytes);
    }

    public static void ReadFromStream<T>(this List<T> data, BinaryReader stream) where T : ISerializable, new()
    {
        var size = stream.ReadInt32();
        data.Capacity = size;
        for (int q = 0; q < size; q++)
        {
            data.Add(stream.ReadSerializable<T>());
        }
    }

    public static uint CalculateHash(this byte[] array)
    {
        uint hash = 0;
        for (int i = 0; i < array.Length; i++)
        {
            hash += array[i];
            hash += hash << 10;
            hash ^= hash >> 6;
        }

        return hash;
    }

    public static ulong CalculateHash(this string array)
    {
        ulong hash = 0;
        for (int i = 0; i < array.Length; i++)
        {
            hash += array[i];
            hash += hash << 15;
            hash ^= hash >> 10;
        }

        return hash;
    }

    public static void WriteToStream<T>(this List<T> data, BinaryWriter stream) where T : ISerializable, new()
    {
        Debug.Assert(data.Count > UInt16.MaxValue, "writing stream failed");
        ushort size = (ushort) data.Count;
        stream.Write(size);
        data.Capacity = size;
        for (int q = 0; q < size; q++)
        {
            stream.Write(data[q]);
        }
    }

    public static void LogCompError<T>(Stack<string> path, string name, T self, T other)
    {
        Debug.LogError($"{path.Reverse().PrintCollection("/")}/{name} is different, self: {self} other: {other}");
    }

    public static void CompareCheck(this byte[] bytes, byte[] bytesOTher, Stack<string> path)
    {
        for (var i = 0; i < bytes.Length; i++)
        {
            var b = bytes[i];
            if (b != bytesOTher[i]) LogCompError(path, i.ToString(), b, bytesOTher[i]);
        }
    }

    // Return if needs to check further
    public static bool CompareNull<T>(Stack<string> path, string name, T val, T val2) where T : class
    {
        if (val == null && val2 == null)
        {
            return false;
        }

        if (val != null && val2 != null)
        {
            return true;
        }

        Func<T, string> pr = t => t == null ? "null" : "not null";
        Debug.LogError($"{path.Reverse().PrintCollection("/")}/{name} is different, self: {pr(val)} other: {pr(val2)}");

        return false;
    }

    public static bool CompareClassId<T>(Stack<string> path, string name, T val, T val2) where T : IPolymorphable
    {
        if (val.GetClassId() != val2.GetClassId())
        {
            Func<T, string> pr = t => t.GetClassId().ToString();
            Debug.LogError(
                $"{path.Reverse().PrintCollection("/")}/{name} class id do not mach, self: {pr(val)} other: {pr(val2)}");
            return false;
        }

        return true;
    }

    public static void WriteJson(this IJsonSerializable obj, JsonTextWriter writer)
    {
        writer.WriteStartObject();
        var polymorph = obj as IPolymorphable;
        if (polymorph != null)
        {
            writer.WritePropertyName("classId");
            writer.WriteValue(polymorph.GetClassId());
//            var f = writer.Formatting;
//            writer.Formatting = Formatting.None;
//            writer.WritePropertyName("className");
//            writer.WriteValue(obj.GetType().Name);
//            writer.Formatting = f;
        }

        obj.WriteJsonFields(writer);
        writer.WriteEndObject();
    }

    public static T ReadAsJsonRoot<T>(this JsonTextReader reader, T obj = null)
        where T : class, IJsonSerializable, new()
    {
        if (obj == null) obj = new T();
        reader.Read();
        obj.ReadFromJson(reader);
        return obj;
    }
    
    public static void ReadFromJson<T>(this List<T> self, JsonTextReader reader, Func<ushort, T> constructor) where T : class, IJsonSerializable
    {
        if (reader.TokenType != JsonToken.StartArray) throw new JsonSerializationException("Bad Json Format");
        while (reader.Read())
        {
            if (reader.TokenType == JsonToken.EndArray) { break; }
            var val = constructor(reader.ReadJsonClassId());
            val.ReadFromJson(reader);
            self.Add(val);
        }
    }
    
    public static void WriteJson<T>(this List<T> self, JsonTextWriter writer) where T : IJsonSerializable
    {
        writer.WriteStartArray();
        for (int i = 0; i < self.Count; i++)
        {
            self[i].WriteJson(writer);
        }
        writer.WriteEndArray();
    }

    public static void ReadFromJson<T>(this T t, JsonTextReader reader) where T : IJsonSerializable
    {
        while (reader.Read())
        {
            if (reader.TokenType == JsonToken.PropertyName)
            {
                var name = (string) reader.Value;
                reader.Read();
                t.ReadFromJsonField(reader, name);
            }
            else if (reader.TokenType == JsonToken.EndObject)
            {
                break;
            }
        }
    }

    public static void ReadAssertPropertyName(this JsonTextReader reader, string prop)
    {
        reader.Read();
        if (reader.TokenType != JsonToken.PropertyName || (string) reader.Value != prop)
        {
            throw new ZergRushException("expected a property with name: " + prop);
        }
    }

    public static ushort ReadJsonClassId(this JsonTextReader reader)
    {
        reader.Read();
        if (reader.TokenType == JsonToken.PropertyName && (string) reader.Value == "classId")
        {
            return (ushort) reader.ReadAsInt32();
        }
        else
        {
            Debug.LogError("error while reading class id in json");
        }

        return 0;
    }

    public static bool CompareRefs<T>(Stack<string> path, string name, T val, T val2)
    {
        if (object.ReferenceEquals(val, val2))
        {
            Debug.LogError($"{path.Reverse().PrintCollection("/")}/{name} class refs do not match");
            return false;
        }

        return true;
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

//    public static void ReadFromStream<T>(this ReactiveCollection<T> collection, BinaryReader stream) where T : ISerializable, new()
//    {
//        var data = new List<T>();
//        data.ReadFromStream(stream);
//        collection.Reset(data);
//    }
//    
//    public static void WriteToStream<T>(this ReactiveCollection<T> data, BinaryWriter stream) where T : ISerializable, new()
//    {
//        stream.Write(data.current);
//    }
}