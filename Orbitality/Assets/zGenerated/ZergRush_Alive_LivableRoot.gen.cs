using System;
using System.Collections.Generic;
using System.Text;
using ZergRush.Alive;
using System.IO;
using Newtonsoft.Json;
#if !INCLUDE_ONLY_CODE_GENERATION
namespace ZergRush.Alive {

    public partial class LivableRoot : IUpdatableFrom<ZergRush.Alive.LivableRoot>, IUpdatableFrom<ZergRush.Alive.DataNode>, IHashable, ICompareChechable<ZergRush.Alive.DataNode>, IJsonSerializable, IPolymorphable
    {
        public override void UpdateFrom(ZergRush.Alive.DataNode other) 
        {
            base.UpdateFrom(other);
            var otherConcrete = (ZergRush.Alive.LivableRoot)other;
        }
        public void UpdateFrom(ZergRush.Alive.LivableRoot other) 
        {
            this.UpdateFrom((ZergRush.Alive.DataNode)other);
        }
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
            return hash;
        }
        public virtual void Enlive() 
        {
            EnliveSelf();
            EnliveChildren();
        }
        public virtual void Mortify() 
        {
            MortifySelf();
            MortifyChildren();
        }
        protected virtual void EnliveChildren() 
        {

        }
        protected virtual void MortifyChildren() 
        {

        }
        public override void __GenIds() 
        {
            base.__GenIds();

        }
        public override void __SetupHierarchy() 
        {
            base.__SetupHierarchy();

        }
        public  LivableRoot() 
        {

        }
        public override void ReadFromJsonField(JsonTextReader reader, string name) 
        {
            base.ReadFromJsonField(reader,name);
            switch(name)
            {
            }
        }
        public override void WriteJsonFields(JsonTextWriter writer) 
        {
            base.WriteJsonFields(writer);

        }
        public override ZergRush.Alive.DataNode NewInst() 
        {
        throw new NotImplementedException();
        }
    }
}
#endif
