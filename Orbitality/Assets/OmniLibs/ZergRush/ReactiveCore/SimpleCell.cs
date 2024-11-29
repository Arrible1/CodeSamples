using System;
using System.Collections;
using System.Collections.Generic;
using ZergRush.Alive;
using ZergRush.Alive;

namespace ZergRush.ReactiveCore 
{
    // Several simple reactive structs for high performance replicative models
    public partial struct LCell<T> where T : IHashable, IUpdatableFrom<T>, ICompareChechable<T>
    {
        public T __val;
        [GenIgnore] private LEvent<T> up;

        public LCell(T t = default(T))
        {
            __val = t;
            up = new LEvent<T>();
        }

        public T value
        {
            get { return __val; }
            set
            {
                if (up.callbacks != null && EqualityComparer<T>.Default.Equals(value, __val) == false)
                {
                    __val = value;
                    up.Send(__val);
                }
                else
                {
                    __val = value;
                }
            }
        }

        public void Bind(Livable owner, Action<T> onNewValue)
        {
            up.Subscribe(owner, onNewValue);
            onNewValue(__val);
        }

        public void SubcribeUpdates(Livable owner, Action<T> onUpdate)
        {
            up.Subscribe(owner, onUpdate);
        }
    }


    [GenIgnore]
    public partial struct LEvent<T>
    {
        public List<Action<T>> callbacks;

        public bool empty => callbacks == null || callbacks.Count == 0;
        public void Subscribe(Livable owner, Action<T> action)
        {
            owner.AddConnection(SubscribeForAdults(action));
        }
        
        public Connection SubscribeForAdults(Action<T> action)
        {
            if (callbacks == null) callbacks = new List<Action<T>>();
            callbacks.Add(action);
            return new Connection {obj = action, reader = callbacks};
        }

        // Subscribtion without connections, be sure you know what you are doing
        public void SubscribeUnsafe(Action<T> action)
        {
            if (callbacks == null) callbacks = new List<Action<T>>();
            callbacks.Add(action);
        }

        public void Send(T t)
        {
            if (callbacks == null) return;
            for (int i = 0; i < callbacks.Count; i++)
            {
                callbacks[i](t);
            }
        }
    }

    [GenTask(GenTaskFlags.SimpleDataPack)]
    public partial struct LEvent
    {
        [GenIgnore] public List<Action> callbacks;

        public void Subscribe(Livable owner, Action action)
        {
            if (callbacks == null) callbacks = new List<Action>();
            callbacks.Add(action);
            owner.AddConnection(new Connection {obj = action, reader = callbacks});
        }

        public void Send()
        {
            if (callbacks == null) return;
            for (int i = 0; i < callbacks.Count; i++)
            {
                callbacks[i]();
            }
        }
    }

}