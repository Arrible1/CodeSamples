using System;
using System.Collections.Generic;
using System.Text;
using ZergRush.Alive;
using System.IO;
using Newtonsoft.Json;
#if !INCLUDE_ONLY_CODE_GENERATION
namespace ZergRush.ReactiveCore {

    public partial struct LCompositeEvent<T> : IUpdatableFrom<ZergRush.ReactiveCore.LCompositeEvent<T>>, IHashable, ICompareChechable<ZergRush.ReactiveCore.LCompositeEvent<T>>
    {
        public void UpdateFrom(ZergRush.ReactiveCore.LCompositeEvent<T> other) 
        {
            uiEvent.UpdateFrom(other.uiEvent);
        }
        public void Deserialize(BinaryReader reader) 
        {
            uiEvent.Deserialize(reader);
        }
        public void Serialize(BinaryWriter writer) 
        {
            uiEvent.Serialize(writer);
        }
        public ulong CalculateHash() 
        {
            System.UInt64 hash = 345093625;
            hash += (ulong)857075900;
            hash += hash << 11; hash ^= hash >> 7;
            hash += uiEvent.CalculateHash();
            hash += hash << 11; hash ^= hash >> 7;
            return hash;
        }
        public void CompareCheck(ZergRush.ReactiveCore.LCompositeEvent<T> other, Stack<string> path) 
        {
            path.Push("uiEvent");
            uiEvent.CompareCheck(other.uiEvent, path);
            path.Pop();
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
                        case "uiEvent":
                        uiEvent.ReadFromJson(reader);
                        break;
                    }
                }
                else if (reader.TokenType == JsonToken.EndObject) { break; }
            }
        }
        public void WriteJson(JsonTextWriter writer) 
        {
            writer.WriteStartObject();
            writer.WritePropertyName("uiEvent");
            uiEvent.WriteJson(writer);
            writer.WriteEndObject();
        }
    }
}
#endif
