using System;
using System.Collections;
using System.Collections.Generic;
using ZergRush.ReactiveCore;

namespace ZergRush.Alive
{
    public partial class DataList<T> : IList<T>, IReadOnlyList<T>
        , IReactiveCollection<T>, IConnectable
        where T : DataNode
        
    {
        public List<T> current => items;
        protected EventStream<ReactiveCollectionEvent<T>> up;
        public IEventReader<ReactiveCollectionEvent<T>> update
        {
            get { return up ?? (up = new EventStream<ReactiveCollectionEvent<T>>()); }
        }

        [GenIgnore] public DataRoot root;
        [GenIgnore] public DataNode carrier;
        [GenIgnore] public LEvent<T> Removed;
        
        protected List<T> items = new List<T>();
        
        public int Capacity
        {
            get => items.Capacity;
            set => items.Capacity = value;
        }
        
        public void ClearDestroyedObjects()
        {
            for (var i = this.Count - 1; i >= 0; i--)
            {
                var obj = this[i];
                if (obj.dead)
                {
                    RemoveAt(i);
                }
            }
        }
        
        public void ForEach(Action<T> action)
        {
            for (var i = 0; i < this.Count; i++)
            {
                var val = this[i];
                action(val);
            }
        }

        public List<T> GetFiltered(Func<T, bool> filter) => items.Filter(filter);
        
        protected virtual void ProcessAddItem(T item)
        {
            if (item.root != root)
            {
                item.root = root;
            }
            item.carrier = carrier;
        }
        
        protected virtual void ProcessRemoveItem(T item)
        {
        }
        
        public IEnumerator<T> GetEnumerator()
        {
            return items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(T item)
        {
            items.Add(item);
            if (item != null)
                ProcessAddItem(item);
            ReactiveCollection<T>.OnItemAdded(item, up, items);
        }

        public void Clear()
        {
            foreach (var item in items)
            {
                ProcessRemoveItem(item);
            }
            var oldItems = items;
            items = new List<T>();
            ReactiveCollection<T>.OnItemsReset(items, oldItems, up);
        }

        public bool Contains(T item)
        {
            return items.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            items.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            int index = items.IndexOf(item);
            var removed = index != -1;                
            if (removed)
                RemoveAt(index);
            return removed;
        }

        public int Count => items.Count;
        public bool IsReadOnly => false;
        public int IndexOf(T item)
        {
            return items.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            items.Insert(index, item);
            if (item != null)
                ProcessAddItem(item);
            ReactiveCollection<T>.OnItemInserted(item, up, index);
        }

        public void RemoveAt(int index)
        {
            var item = items[index];
            ProcessRemoveItem(item);
            items.RemoveAt(index);
            Removed.Send(item);
            ReactiveCollection<T>.OnItemRemovedAt(index, up, item);
        }

        public T this[int index]
        {
            get { return items[index]; }
            set
            {
                var currItem = items[index];
                if (ReferenceEquals(value, currItem)) return;
                
                if (currItem != null)
                    ProcessRemoveItem(currItem);
                items[index] = value;
                if (value != null)
                    ProcessAddItem(value);
                ReactiveCollection<T>.OnItemSet(index, value, currItem, up);
            }
        }

        public int getConnectionCount => up != null ? up.getConnectionCount : 0;

        public void __GenIds()
        {
            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
            }
        }

        public void __SetupHierarchy()
        {
            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                item.root = root;
                item.carrier = carrier;
            }
        }
    }

    public sealed partial class LivableList<T> : DataList<T> where T : Livable
    {
        [GenIgnore] bool alive;

        public void Enlive()
        {
            if (alive)
            {
                throw new Exception("You can not enlive living");
            }

            alive = true;
            for (var i = 0; i < items.Count; i++)
            {
                items[i].Enlive();
            }
        }

        public void Mortify()
        {
            if (!alive)
            {
                throw new Exception("You can not mortify dead");
            }

            for (var i = 0; i < items.Count; i++)
            {
                items[i].Mortify();
            }

            alive = false;
        }

        protected override void ProcessRemoveItem(T item)
        {
            base.ProcessRemoveItem(item);
            if (alive)
            {
                item.Mortify();
            }
            if (root.pool != null) item.ReturnToPool(root.pool);
        }

        protected override void ProcessAddItem(T item)
        {
            base.ProcessAddItem(item);
            if (alive)
            {
                item.Enlive();
            }
        }

        public void OnReturnToPool(ObjectPool pool)
        {
            if (alive)
            {
                throw new ZergRushException($"this method should not be called on alive list");
            }

            foreach (var item in items)
            {
                item.ReturnToPool(pool);
            }

            items.Clear();
        }
    }
}