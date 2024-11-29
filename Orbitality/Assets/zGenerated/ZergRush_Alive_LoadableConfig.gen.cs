using System;
using System.Collections.Generic;
using System.Text;
using ZergRush.Alive;
using System.IO;
#if !INCLUDE_ONLY_CODE_GENERATION
namespace ZergRush.Alive {

    public partial class LoadableConfig : IHashable, IUniquelyIdentifiable, IPolymorphable
    {
        public enum Types : ushort
        {
            LoadableConfig = 1,
            StubTypeBasedDataFromConfig = 2,
        }
        static Func<LoadableConfig> [] polymorphConstructors = new Func<LoadableConfig> [] {
            () => null, // 0
            () => new ZergRush.Alive.LoadableConfig(), // 1
            () => new ZergRush.Alive.StubTypeBasedDataFromConfig(), // 2
        };
        public static LoadableConfig CreatePolymorphic(System.UInt16 typeId) {
            return polymorphConstructors[typeId]();
        }
        public virtual void Deserialize(BinaryReader reader) 
        {

        }
        public virtual void Serialize(BinaryWriter writer) 
        {

        }
        public virtual ulong CalculateHash() 
        {
            System.UInt64 hash = 345093625;
            hash += (ulong)2077598980;
            hash += hash << 11; hash ^= hash >> 7;
            return hash;
        }
        public virtual ulong UId() 
        {
            System.UInt64 hash = GetClassId();
            return hash;
        }
        public virtual void CollectConfigs(ConfigStorageDictionary _collection) 
        {

        }
        public  LoadableConfig() 
        {

        }
        public virtual ushort GetClassId() 
        {
        return (System.UInt16)Types.LoadableConfig;
        }
        public virtual ZergRush.Alive.LoadableConfig NewInst() 
        {
        return new LoadableConfig();
        }
    }
}
#endif
