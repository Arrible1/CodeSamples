using System;
using ZergRush.Alive;
using ZergRush.ReactiveCore;

namespace ZergRush.Alive
{
    /*
     *     Base class for data node that knows about hierarchy it's being contained in
     *     Also it could be marked as destroyed for containers like DataList to delete it
     *     Data node can have reference id to be referenced from other parts of data tree
     *     To make it referensable add "int id;" field and [HasRefId] tag on the class
     */
    [GenTask(GenTaskFlags.NodePack)]
    public abstract partial class DataNode : IDataNode
    {
        [GenInclude] public bool dead;
        [GenIgnore] public DataRoot root;
        [GenIgnore] public DataNode carrier;

        public void Destroy()
        {
            if (dead)
                return;
            dead = true;
            if (_deadEvent != null)
            {
                _deadEvent.Send();
                _deadEvent.ClearCallbacks();
            }

            Die();
        }

        public virtual void Die()
        {
        }

        public bool ShouldBeDestroyed => dead;

        public IEventReader deadEvent
        {
            get
            {
                if (_deadEvent == null) _deadEvent = new EventStream();
                return _deadEvent;
            }
        }

        public virtual void ReturnToPool(ObjectPool pool)
        {
            throw new NotImplementedException();
        }

        [GenIgnore] EventStream _deadEvent;
    }

    public interface IDataNode
    {
        bool ShouldBeDestroyed { get; }
        IEventReader deadEvent { get; }
    }
}