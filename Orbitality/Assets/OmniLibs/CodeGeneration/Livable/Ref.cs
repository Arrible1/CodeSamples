using System;
using Newtonsoft.Json;
using ZergRush.ReactiveCore;

namespace ZergRush.Alive
{
    [GenTask(GenTaskFlags.CompareChech), GenTaskCustomImpl(GenTaskFlags.PooledUpdateFrom | GenTaskFlags.JsonSerialization | GenTaskFlags.DefaultConstructor)]
    public sealed partial class Ref<T> : DataNode, ICell<T>, IConnectable
        where T : class, IDataNode, IReferencableFromDataRoot
    {
        int _id;
        [GenIgnore] T cachedVal;
        
        [GenIgnore] private EventStream<T> up;
        public IDisposable ListenUpdates(Action<T> reaction)
        {
            if (up == null) up = new EventStream<T>();
            return up.Subscribe(reaction);
        }

        public int id
        {
            get
            {
                if (_id == 0 && cachedVal != null)
                {
                    throw new NotImplementedException();
                }
                return _id;
            }
            set 
            { 
                _id = value;
                cachedVal = null;
            }
        }

        public T value
        {
            get
            {
                if (cachedVal == null)
                {
                    if (_id == 0) return null;
                    var recolled = root.RecallMayBe(_id);
                    if (recolled == null)
                    {
                        //_id = 0;
                        return null;
                    }
                    if (recolled.dead)
                    {
                        return null;
                    }
                    cachedVal = recolled as T;
                    if (cachedVal == null)
                    {
                        throw new ZergRushException("invalid object stored with id: " + _id);
                    }
                }
                else if (_id != cachedVal.Id)
                {
                    cachedVal = null;
                }
                else if (cachedVal.ShouldBeDestroyed)
                {
                    return null;
                }

                return cachedVal;
            }
            set
            {
                if (value == null)
                {
                    _id = 0;
                    cachedVal = null;
                }
                else
                {
                    cachedVal = value;
                    _id = value.Id;
                }

                if (up != null) { up.Send(value); }
            }
        }

        public static implicit operator T(Ref<T> r)
        {
            return r.value;
        }

        public int getConnectionCount => up == null ? 0 : up.getConnectionCount;
    }
}

