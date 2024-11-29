using System;
using System.Collections.Generic;
using System.Text;
using ZergRush.Alive;
using System.IO;
using Newtonsoft.Json;
#if !INCLUDE_ONLY_CODE_GENERATION
namespace ZergRush.ReactiveCore {

    public partial struct EventBuffer<T> : IUpdatableFrom<ZergRush.ReactiveCore.EventBuffer<T>>, IHashable, ICompareChechable<ZergRush.ReactiveCore.EventBuffer<T>>
    {
        public void UpdateFrom(ZergRush.ReactiveCore.EventBuffer<T> other) 
        {

        }
        public void Deserialize(BinaryReader reader) 
        {

        }
        public void Serialize(BinaryWriter writer) 
        {

        }
        public ulong CalculateHash() 
        {
            System.UInt64 hash = 345093625;
            hash += (ulong)758861549;
            hash += hash << 11; hash ^= hash >> 7;
            return hash;
        }
        public void CompareCheck(ZergRush.ReactiveCore.EventBuffer<T> other, Stack<string> path) 
        {

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
                    }
                }
                else if (reader.TokenType == JsonToken.EndObject) { break; }
            }
        }
        public void WriteJson(JsonTextWriter writer) 
        {
            writer.WriteStartObject();
            writer.WriteEndObject();
        }
    }
}
#endif
