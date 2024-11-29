using System;
using System.IO;
using ZergRush.Alive;
using TWL;
using ZergRush.ReactiveCore;

namespace ZergRush.Alive
{
    [GenTaskCustomImpl(GenTaskFlags.NodePack)]
    public sealed partial class DataSlot<TLivable> : DataNode, ICell<TLivable>, IConnectable  where TLivable : DataNode
    {
        [CanBeNull] TLivable _value;

        public TLivable value
        {
            get { return _value; }
            set
            {
                if (_value == value) return;

                this._value = value;

                if (_value != null)
                {
                    if (_value.root != root)
                    {
                        _value.root = root;
                    }
                    _value.carrier = carrier;
                }

                if (up != null)
                    up.Send(_value);
            }
        }

        [GenIgnore] private EventStream<TLivable> up;
        public IDisposable ListenUpdates(Action<TLivable> reaction)
        {
            if (up == null) up = new EventStream<TLivable>();
            return up.Subscribe(reaction);
        }

        public int getConnectionCount => up != null ? up.getConnectionCount : 0;
    }

    [GenTaskCustomImpl(GenTaskFlags.LivableNodePack)]
    public sealed partial class LivableSlot<TLivable> : Livable where TLivable : Livable
    {
        [CanBeNull] TLivable _value;
        [GenIgnore] public Action<TLivable> onBeforeEnlive;
        [GenIgnore] public Func<TLivable> createValue;
        
        public void CreateValue()
        {
            value = createValue();
        }
        public void ClearValue()
        {
            value = null;
        }
                
        public override void Enlive()
        {
            EnliveSelf();
            EnliveValue();
        }

        public override void Mortify()
        {
            MortifySelf();
            _value?.Mortify();
        }

        public override void Deserialize(BinaryReader reader)
        {
            throw new NotImplementedException();
        }

        void EnliveValue()
        {
            if (_value == null) return;
            onBeforeEnlive?.Invoke(_value);
            _value.Enlive();
        }

        public TLivable value
        {
            get { return _value; }
            set
            {
                if (_value == value) return;
                
                if (_value != null)
                {
                    if (alive)
                        _value.Mortify();
                    _value.ReturnToPool(root.pool);
                }

                this._value = value;
                
                if (_value == null) return;

                if (_value.alive)
                {
                    throw new ZergRushException("alive value came into livable slot");
                }
                
                if (_value.root != root)
                {
                    _value.root = root;
                }
                _value.carrier = carrier;
                if (alive)
                {
                    EnliveValue();
                }
            }
        }

        public void OnReturnToPool(ObjectPool pool)
        {
            _value?.ReturnToPool(pool);
            _value = null;
        }

        public void TransplantTo(LivableSlot<TLivable> otherSlotOfSameParent)
        {
            var temp = _value;
            _value = null;
            otherSlotOfSameParent._value = temp;
        }

    }
}