using System;
using System.Collections.Generic;
using System.Text;
using ZergRush.Alive;
using System.IO;
using Newtonsoft.Json;
#if !INCLUDE_ONLY_CODE_GENERATION
namespace ZergRush.Alive {

    public partial struct LCellInt : IUpdatableFrom<ZergRush.Alive.LCellInt>, IHashable, ICompareChechable<ZergRush.Alive.LCellInt>
    {
        public void UpdateFrom(ZergRush.Alive.LCellInt other) 
        {
            __val = other.__val;
        }
        public void Deserialize(BinaryReader reader) 
        {
            __val = reader.ReadInt32();
        }
        public void Serialize(BinaryWriter writer) 
        {
            writer.Write(__val);
        }
        public ulong CalculateHash() 
        {
            System.UInt64 hash = 345093625;
            hash += (ulong)428025537;
            hash += hash << 11; hash ^= hash >> 7;
            hash += (System.UInt64)__val;
            hash += hash << 11; hash ^= hash >> 7;
            return hash;
        }
        public void CompareCheck(ZergRush.Alive.LCellInt other, Stack<string> path) 
        {
            if (__val != other.__val) SerializationTools.LogCompError(path, "__val", other.__val, __val);
        }
        public void ReadFromJson(JsonTextReader reader) 
        {
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    var name = (string) reader.Value;
                    reader.Read();
                    switch(name)
                    {
                        case "__val":
                        __val = (int)(Int64)reader.Value;
                        break;
                    }
                }
                else if (reader.TokenType == JsonToken.EndObject) { break; }
            }
        }
        public void WriteJson(JsonTextWriter writer) 
        {
            writer.WriteStartObject();
            writer.WritePropertyName("__val");
            writer.WriteValue(__val);
            writer.WriteEndObject();
        }
    }
}
#endif
