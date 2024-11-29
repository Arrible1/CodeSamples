using System;
using System.Collections.Generic;
using System.Text;
using ZergRush.Alive;
using System.IO;
using Newtonsoft.Json;
#if !INCLUDE_ONLY_CODE_GENERATION
namespace ZergRush.Alive {

    public partial class DataNode : IUpdatableFrom<ZergRush.Alive.DataNode>, IHashable, ICompareChechable<ZergRush.Alive.DataNode>, IJsonSerializable, IPolymorphable
    {
        public enum Types : ushort
        {
        }
        public virtual System.UInt16 GetClassId(){throw new NotImplementedException();}
        static Func<DataNode> [] polymorphConstructors = new Func<DataNode> [] {
        };
        public static DataNode CreatePolymorphic(System.UInt16 typeId) {
            return polymorphConstructors[typeId]();
        }
        public virtual void UpdateFrom(ZergRush.Alive.DataNode other) 
        {
            dead = other.dead;
        }
        public virtual void Deserialize(BinaryReader reader) 
        {
            dead = reader.ReadBoolean();
        }
        public virtual void Serialize(BinaryWriter writer) 
        {
            writer.Write(dead);
        }
        public virtual ulong CalculateHash() 
        {
            System.UInt64 hash = 345093625;
            hash += dead ? 1u : 0u;
            hash += hash << 11; hash ^= hash >> 7;
            return hash;
        }
        public virtual void __GenIds() 
        {

        }
        public virtual void __SetupHierarchy() 
        {

        }
        public  DataNode() 
        {

        }
        public virtual void CompareCheck(ZergRush.Alive.DataNode other, Stack<string> path) 
        {
            if (dead != other.dead) SerializationTools.LogCompError(path, "dead", other.dead, dead);
        }
        public virtual void ReadFromJsonField(JsonTextReader reader, string name) 
        {
            switch(name)
            {
                case "dead":
                dead = (System.Boolean)reader.Value;
                break;
            }
        }
        public virtual void WriteJsonFields(JsonTextWriter writer) 
        {
            writer.WritePropertyName("dead");
            writer.WriteValue(dead);
        }
        public virtual ZergRush.Alive.DataNode NewInst() 
        {
        throw new NotImplementedException();
        }
    }
}
#endif
