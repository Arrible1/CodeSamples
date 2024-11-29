using System;
using System.Collections.Generic;
using System.Text;
using ZergRush.Alive;
using System.IO;
using Newtonsoft.Json;
#if !INCLUDE_ONLY_CODE_GENERATION
namespace ZergRush.ReactiveCore {

    public partial class ModifiableLivableList<T> : IHashable, ICompareChechable<ZergRush.Alive.DataNode>, IJsonSerializable, IPolymorphable
    {
        public override ulong CalculateHash() 
        {
            var baseVal = base.CalculateHash();
            System.UInt64 hash = baseVal;
            hash += (ulong)1395510972;
            hash += hash << 11; hash ^= hash >> 7;
            hash += collection.CalculateHash();
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
        public  ModifiableLivableList() 
        {
            collection = new ZergRush.ReactiveCore.ReactiveCollection<T>();
        }
        public override void CompareCheck(ZergRush.Alive.DataNode other, Stack<string> path) 
        {
            base.CompareCheck(other,path);
            var otherConcrete = (ZergRush.ReactiveCore.ModifiableLivableList<T>)other;
            path.Push("collection");
            collection.CompareCheck(otherConcrete.collection, path);
            path.Pop();
        }
        public override void ReadFromJsonField(JsonTextReader reader, string name) 
        {
            base.ReadFromJsonField(reader,name);
            switch(name)
            {
                case "collection":
                collection.ReadFromJson(reader);
                break;
            }
        }
        public override void WriteJsonFields(JsonTextWriter writer) 
        {
            base.WriteJsonFields(writer);
            writer.WritePropertyName("collection");
            collection.WriteJson(writer);
        }
        public override ushort GetClassId() 
        {
        return 0;
        }
        public override ZergRush.Alive.DataNode NewInst() 
        {
        return new ModifiableLivableList<T>();
        }
    }
}
#endif
