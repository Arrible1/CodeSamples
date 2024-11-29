using System;
using System.Collections.Generic;
using System.Text;
using ZergRush.Alive;
using System.IO;
#if !INCLUDE_ONLY_CODE_GENERATION
namespace ZergRush.Alive {

    public partial class StubTypeBasedDataFromConfig : IHashable, IUniquelyIdentifiable, IPolymorphable
    {
        public override void Deserialize(BinaryReader reader) 
        {
            base.Deserialize(reader);

        }
        public override void Serialize(BinaryWriter writer) 
        {
            base.Serialize(writer);

        }
        public override ulong CalculateHash() 
        {
            var baseVal = base.CalculateHash();
            System.UInt64 hash = baseVal;
            hash += (ulong)832556751;
            hash += hash << 11; hash ^= hash >> 7;
            return hash;
        }
        public override ulong UId() 
        {
            System.UInt64 hash = GetClassId();
            return hash;
        }
        public override void CollectConfigs(ConfigStorageDictionary _collection) 
        {
            base.CollectConfigs(_collection);

        }
        public  StubTypeBasedDataFromConfig() 
        {

        }
        public override ushort GetClassId() 
        {
        return (System.UInt16)Types.StubTypeBasedDataFromConfig;
        }
        public override ZergRush.Alive.LoadableConfig NewInst() 
        {
        return new StubTypeBasedDataFromConfig();
        }
    }
}
#endif
