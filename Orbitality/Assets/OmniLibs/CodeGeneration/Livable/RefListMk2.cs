using System;
using System.Collections;
using System.Collections.Generic;
using ZergRush.ReactiveCore;

namespace ZergRush.Alive
{
    public sealed partial class RefListMk2<T> : IReactiveCollection<T> where T : class, IDataNode, IReferencableFromDataRoot
    {
        protected List<T> data;
        List<int> ids = new List<int>();

        public DataRoot root
        {
            get { return _root; }
            set
            {
                _root = value;
                SetUp();
            }
        }
        
        public void ClearDead()
        {
            for (var i = ids.Count - 1; i >= 0; i--)
            {
                if(root.RecallMayBe(ids[i]) == null) RemoveAt(i);
            }
        }

        void SetUp()
        {
            data.Clear();
            data.Capacity = ids.Count;
            for (var i = 0; i < ids.Count; i++)
            {
                var entity = _root.Recall(ids[i]);
                if (entity is T t)
                {
                    data.Add(t);
                }
                else
                {
                    throw new ZergRushException("entity recalled is other type");
                }
            }
        }
        
        [GenIgnore] public DataRoot _root;
        [GenIgnore] public DataNode carrier;

        protected EventStream<ReactiveCollectionEvent<T>> up;
        public int Count => ids.Count;

        public List<T> current
        {
            get { return data; }
            set { Reset(value); }
        }

        public RefListMk2()
        {
            this.data = new List<T>();
        }

        public void AddRange(IEnumerable<T> items)
        {
            foreach (var item in items) Add(item);
        }

        public IEventReader<ReactiveCollectionEvent<T>> update
        {
            get { return up ?? (up = new EventStream<ReactiveCollectionEvent<T>>()); }
        }

        public bool Contains(T item)
        {
            CheckRoot();
            return data.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            for (int i = arrayIndex; i < data.Count + arrayIndex; i++)
            {
                array[i] = data[i - arrayIndex];
            }
        }

        void OnItemAdd(T item)
        {
            item.deadEvent.Subscribe(() => Remove(item));
        }

        void OnItemRemoved(T item)
        {
            //so we need to store subscribtion for proper unsubscribe,
            //but if we not we'll have multiple removes which is not that bad for now
        }

        public void Add(T item)
        {
            ids.Add(item.Id);
            data.Add(item);
            OnItemAdd(item);
            ReactiveCollection<T>.OnItemAdded(item, up, data);
        }

        public int IndexOf(T item)
        {
            return data.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            data.Insert(index, item);
            OnItemAdd(item);
            ReactiveCollection<T>.OnItemInserted(item, up, index);
        }

        public bool Remove(T item)
        {
            var index = data.IndexOf(item);
            if (index >= 0)
            {
                RemoveAt(index);
                return true;
            }
            return false;
        }
        
        public void Clear()
        {
            Reset(new List<T>());
        }

        public int RemoveAll(Func<T, bool> predicate)
        {
            int removedCounter = 0;
            for (int i = data.Count - 1; i >= 0; i--)
            {
                var item = data[i];
                if (predicate(item))
                {
                    removedCounter++;
                    RemoveAt(i);
                }
            }

            return removedCounter;
        }

        public void RemoveRange(int ind, int count)
        {
            for (int i = 0; i < count; i++)
                RemoveAt(ind);
        }

        public void RemoveAt(int index)
        {
            var item = data[index];
            data.RemoveAt(index);
            OnItemRemoved(item);
            ReactiveCollection<T>.OnItemRemovedAt(index, up, item);
        }
        
        public void Reset(List<T> newData)
        {
            var oldData = data;
            data = newData;
            ReactiveCollection<T>.OnItemsReset(newData, oldData, up);
        }
        
        public int IdAtIndex(int index)
        {
            return ids[index];
        }

        public void Reset(IEnumerable<T> val = null)
        {
            var oldData = data;
            data = val != null ? new List<T>(val) : new List<T>();
            if (oldData.Count == 0 && data.Count == 0) return;
            ReactiveCollection<T>.OnItemsReset(data, oldData, up);
        }

        public T this[int index]
        {
            get { return data[index]; }
            set
            {
                var oldItem = data[index];
                data[index] = value;
                OnItemAdd(value);
                ReactiveCollection<T>.OnItemSet(index, value, oldItem, up);
            }
        }

        public int Capacity
        {
            get { return data.Capacity; }
            set { data.Capacity = value; }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override string ToString()
        {
            return current.PrintCollection();
        }
        
        public int getConnectionCount => up == null ? 0 : up.getConnectionCount;
        void CheckRoot()
        {
            if (_root == null) throw new ZergRushException("root is not set");
        }

        public void UpdateFrom(RefListMk2<T> other)
        {
            ids.Clear();
            ids.AddRange(other.ids);
            if (_root != null) SetUp();
        }

        public void CompareCheck(RefListMk2<T> other, Stack<string> path)
        {
            if (Count != other.Count) SerializationTools.LogCompError(path, "Count", other.Count, Count);
            var count = Math.Min(Count, other.Count);
            for (int i = 0; i < count; i++)
            {
                if (ids[i] != other.ids[i]) SerializationTools.LogCompError(path, $"id at index: {i.ToString()}", ids[i], other.ids[i]);
            }
        }
        
        public ulong CalculateHash()
        {
            ulong hash = 0xffffff;
            for (int i = 0; i < Count; i++)
            {
                hash += (ulong)ids[i];
                hash += hash << 11; hash ^= hash >> 7;
            }
            return hash;
        }

    }
}