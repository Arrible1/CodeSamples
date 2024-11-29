using System;
using System.Collections.Generic;
using System.Text;
using ZergRush.Alive;
using System.IO;
#if !INCLUDE_ONLY_CODE_GENERATION
namespace ZergRush.Alive {

    public partial class Ref<T> : IUpdatableFrom<ZergRush.Alive.Ref<T>>, IUpdatableFrom<ZergRush.Alive.DataNode>, IHashable, ICompareChechable<ZergRush.Alive.DataNode>, IPolymorphable
    {
        public override void UpdateFrom(ZergRush.Alive.DataNode other) 
        {
            base.UpdateFrom(other);
            var otherConcrete = (ZergRush.Alive.Ref<T>)other;
            _id = otherConcrete._id;
        }
        public void UpdateFrom(ZergRush.Alive.Ref<T> other) 
        {
            this.UpdateFrom((ZergRush.Alive.DataNode)other);
        }
        public override void Deserialize(BinaryReader reader) 
        {
            base.Deserialize(reader);
            _id = reader.ReadInt32();
        }
        public override void Serialize(BinaryWriter writer) 
        {
            base.Serialize(writer);
            writer.Write(_id);
        }
        public override ulong CalculateHash() 
        {
            var baseVal = base.CalculateHash();
            System.UInt64 hash = baseVal;
            hash += (ulong)1940509952;
            hash += hash << 11; hash ^= hash >> 7;
            hash += (System.UInt64)_id;
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
        public override void CompareCheck(ZergRush.Alive.DataNode other, Stack<string> path) 
        {
            base.CompareCheck(other,path);
            var otherConcrete = (ZergRush.Alive.Ref<T>)other;
            if (_id != otherConcrete._id) SerializationTools.LogCompError(path, "_id", otherConcrete._id, _id);
        }
        public override ushort GetClassId() 
        {
        return 0;
        }
        public override ZergRush.Alive.DataNode NewInst() 
        {
        return new Ref<T>();
        }
    }
}
#endif
