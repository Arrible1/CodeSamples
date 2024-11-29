using System;
using System.Collections.Generic;
using ZergRush.ReactiveCore;

namespace ZergRush.Alive
{
    [GenSimpleData]
    public partial struct LCellInt
    {
        public int __val;
        [GenIgnore] private LEvent<int> up;

        public LCellInt(int t = default(int))
        {
            __val = t;
            up = new LEvent<int>();
        }

        public int value
        {
            get { return __val; }
            set
            {
                if (up.callbacks != null && value != __val)
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

        public void Bind(Livable owner, Action<int> onNewValue)
        {
            up.Subscribe(owner, onNewValue);
            onNewValue(__val);
        }

        public void SubcribeUpdates(Livable owner, Action<int> onUpdate)
        {
            up.Subscribe(owner, onUpdate);
        }
    }
}