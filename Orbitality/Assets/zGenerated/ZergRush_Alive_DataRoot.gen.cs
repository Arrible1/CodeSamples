using System;
using System.Collections.Generic;
using System.Text;
using ZergRush.Alive;
using System.IO;
using Newtonsoft.Json;
#if !INCLUDE_ONLY_CODE_GENERATION
namespace ZergRush.Alive {

    public partial class DataRoot : IUpdatableFrom<ZergRush.Alive.DataRoot>, IUpdatableFrom<ZergRush.Alive.DataNode>, IHashable, ICompareChechable<ZergRush.Alive.DataNode>, IJsonSerializable, IPolymorphable
    {
        public void BaseUpdateFrom(ZergRush.Alive.DataNode other) 
        {
            base.UpdateFrom(other);
            var otherConcrete = (ZergRush.Alive.DataRoot)other;
            __entityIdFactory = otherConcrete.__entityIdFactory;
        }
        public void BaseUpdateFrom(ZergRush.Alive.DataRoot other) 
        {
            this.UpdateFrom((ZergRush.Alive.DataNode)other);
        }
        public override void Deserialize(BinaryReader reader) 
        {
            base.Deserialize(reader);
            __entityIdFactory = reader.ReadInt32();
        }
        public override void Serialize(BinaryWriter writer) 
        {
            base.Serialize(writer);
            writer.Write(__entityIdFactory);
        }
        public override ulong CalculateHash() 
        {
            var baseVal = base.CalculateHash();
            System.UInt64 hash = baseVal;
            hash += (System.UInt64)__entityIdFactory;
            hash += hash << 11; hash ^= hash >> 7;
            return hash;
        }
        public override void __GenIds() 
        {
            base.__GenIds();

        }
        public override void __SetupHierarchy() 
        {
            base.__SetupHierarchy();

        }
        public  DataRoot() 
        {

        }
        public override void CompareCheck(ZergRush.Alive.DataNode other, Stack<string> path) 
        {
            base.CompareCheck(other,path);
            var otherConcrete = (ZergRush.Alive.DataRoot)other;
            if (__entityIdFactory != otherConcrete.__entityIdFactory) SerializationTools.LogCompError(path, "__entityIdFactory", otherConcrete.__entityIdFactory, __entityIdFactory);
        }
        public override void ReadFromJsonField(JsonTextReader reader, string name) 
        {
            base.ReadFromJsonField(reader,name);
            switch(name)
            {
                case "__entityIdFactory":
                __entityIdFactory = (int)(Int64)reader.Value;
                break;
            }
        }
        public override void WriteJsonFields(JsonTextWriter writer) 
        {
            base.WriteJsonFields(writer);
            writer.WritePropertyName("__entityIdFactory");
            writer.WriteValue(__entityIdFactory);
        }
        public override ZergRush.Alive.DataNode NewInst() 
        {
        throw new NotImplementedException();
        }
    }
}
#endif
