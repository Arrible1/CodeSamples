using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ZergRush.Alive;

namespace ZergRush.ReactiveCore
{
    [GenLivable, GenTaskCustomImpl(GenTaskFlags.Serialization | GenTaskFlags.UpdateFrom | GenTaskFlags.LifeSupport)]
    public sealed partial class ModifiableLivableList<T> : Livable, IReactiveCollection<T>,  IEnumerable
        where T : Livable, ILivableModification, IHashable
    {
        readonly ReactiveCollection<T> collection;

        // TODO need some work for proper saving and updatingFrom
        [GenIgnore(GenTaskFlags.All & ~GenTaskFlags.Serialization)]
        Dictionary<int, List<T>> savedData;

        public void ModifyAdd(IConnectionSink sink, T newAbilityInstance, int modificationOwner)
        {
            newAbilityInstance.root = root;
            newAbilityInstance.carrier = carrier;
            
            if (savedData != null && savedData.TryGetValue(modificationOwner, out var list))
            {
                var itemIndex = list.FindIndex(item => item.UId() == newAbilityInstance.UId());
                if (itemIndex != -1)
                {
                    // old save item from previous save
                    var item = list[itemIndex];
                    // is no longer needed and can be removed
                    list.RemoveAt(itemIndex);
                    if (list.Count == 0)
                    {
                        savedData.Remove(modificationOwner);
                    }
                }
            }
            else
            {
            }

            collection.Add(newAbilityInstance);
            newAbilityInstance.Enlive();
            sink.AddConnection(new AnonymousDisposable(() => collection.Remove(newAbilityInstance)));
        }

        public IEnumerator<T> GetEnumerator()
        {
            return collection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) collection).GetEnumerator();
        }

        public IEventReader<ReactiveCollectionEvent<T>> update => collection.update;
        public List<T> current => collection.current;
        public int Count => collection.Count;
        public T this[int index] => collection[index];
        public override void Enlive()
        {
            throw new NotImplementedException();
        }

        public override void Mortify()
        {
            throw new NotImplementedException();
        }

        public override void Deserialize(BinaryReader reader)
        {
            throw new NotImplementedException();
        }
    }

    public abstract class Modifiable<TVal, TModification> : ICell<TVal>
    {
        public Modifiable(TVal baseVal)
        {
            this.baseVal = baseVal;
            this.currVal = baseVal;
        }

        protected TVal baseVal;
        protected TVal currVal;
        protected List<TModification> modifications = new List<TModification>();
        EventStream<TVal> changed = new EventStream<TVal>();

        public TVal baseValue
        {
            get { return baseVal; }
            set
            {
                baseVal = value;
                Update();
            }
        }

        private void AddModification(TModification mod)
        {
            modifications.Add(mod);
            Update();
        }

        private void RemoveModification(TModification mod)
        {
            modifications.Remove(mod);
            Update();
        }

        private void ReplaceModification(TModification modOld, TModification modNew)
        {
            var index = modifications.FindIndex(m => EqualityComparer<TModification>.Default.Equals(m, modOld));
            modifications[index] = modNew;
            Update();
        }

        private Action ModifyRaw(TModification mod)
        {
            AddModification(mod);
            return () => RemoveModification(mod);
        }

        public IDisposable Modify(TModification mod)
        {
            return ModifyRaw(mod).ToDisposable();
        }

        public IDisposable Modify(ICell<TModification> cellMod)
        {
            AddModification(cellMod.value);
            var disp = new DoubleDisposable();
            disp.First = new AnonymousDisposable(() => RemoveModification(cellMod.value));
            disp.Second = cellMod.BufferListenUpdates((newVal, oldVal) => ReplaceModification(oldVal, newVal));
            return disp;
        }

        void Update()
        {
            var newVal = Calculate();
            if (EqualityComparer<TVal>.Default.Equals(newVal, currVal) == false)
            {
                currVal = newVal;
                changed.Send(currVal);
            }
        }

        protected abstract TVal Calculate();

        public IDisposable ListenUpdates(Action<TVal> reaction)
        {
            return changed.Subscribe(reaction);
        }

        public TVal value
        {
            get { return currVal; }
        }
    }

    public static class ModifiableTools
    {
        public static IDisposable ToDisposable(this Action action)
        {
            return new AnonymousDisposable(action);
        }
    }

    public class LastValueModification<T> : Modifiable<T, T>
    {
        public LastValueModification(T baseVal) : base(baseVal)
        {
        }

        protected override T Calculate()
        {
            if (modifications.Count > 0)
                return modifications[modifications.Count - 1];
            else
                return baseVal;
        }
    }

    public class MultiplicativeModification : Modifiable<float, float>
    {
        public MultiplicativeModification() : base(1f)
        {
        }

        protected override float Calculate()
        {
            var result = baseVal;
            for (var i = 0; i < modifications.Count; i++)
            {
                result *= modifications[i];
            }

            return result;
        }
    }

    public class OrModification : Modifiable<bool, bool>
    {
        protected override bool Calculate()
        {
            for (var i = 0; i < modifications.Count; i++)
            {
                if (modifications[i]) return true;
            }

            return false;
        }

        public OrModification() : base(false)
        {
        }
    }

    public class AdditiveModification : Modifiable<float, float>
    {
        public AdditiveModification() : base(0)
        {
        }

        public AdditiveModification(float val) : base(val)
        {
        }

        protected override float Calculate()
        {
            var result = baseVal;
            for (var i = 0; i < modifications.Count; i++)
            {
                result += modifications[i];
            }

            return result;
        }
    }

    public class ModifiableList<T> : IReactiveCollection<T>, IReadOnlyList<T>
    {
        ReactiveCollection<T> collection = new ReactiveCollection<T>();

        public IDisposable ModifyAdd(T elem)
        {
            collection.Add(elem);
            return new AnonymousDisposable(() => collection.Remove(elem));
        }

        public void ModifyAdd(IConnectionSink sink, T elem)
        {
            collection.Add(elem);
            sink.AddConnection(new AnonymousDisposable(() => collection.Remove(elem)));
        }

        public IEnumerator<T> GetEnumerator()
        {
            return collection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) collection).GetEnumerator();
        }

        public IEventReader<ReactiveCollectionEvent<T>> update => collection.update;
        public List<T> current => collection.current;
        public int Count => collection.Count;
        public T this[int index] => collection[index];
    }
}