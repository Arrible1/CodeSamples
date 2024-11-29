using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using ServerEngine;
using ZergRush.Alive;

namespace ZergRush.ReactiveCore
{
    public interface IEventReader<out T> : IEventReader
    {
        IDisposable Subscribe(Action<T> action);
    }

    public interface IEventWriter<in T>
    {
        void Send(T val);
    }

    public interface IEventRW<T> : IEventReader<T>, IEventWriter<T>
    {
    }


    public class EventStream<T> : IEventRW<T>, IConnectable
    {
        List<Action<T>> callbacks;
        bool iterating;
        ValueListItem nextValue;

        class ValueListItem
        {
            public T item;
            public List<Action<T>> callbacks;
            public ValueListItem next;
        }

        class Disconnect : IDisposable
        {
            public EventStream<T> reader;
            public Action<T> action;

            public void Dispose()
            {
                if (reader != null)
                {
                    reader.RemoveListener(action);
                    reader = null;
                    action = null;
                }
            }
        }

        void RemoveListener(Action<T> action)
        {
            if (iterating)
            {
                callbacks = callbacks.ToList();
            }
            callbacks.Remove(action);
        }

        public IDisposable Subscribe(Action<T> action)
        {
            if (callbacks == null) callbacks = new List<Action<T>>();
            else if (iterating) { callbacks = callbacks.ToList(); }
            callbacks.Add(action);
            return new Disconnect { reader = this, action = action };
        }

        public void Send(T t)
        {
            if (callbacks == null) return;

            if (iterating)
            {
                var newItem = new ValueListItem
                {
                    item = t,
                    callbacks = callbacks
                };
                if (nextValue == null)
                {
                    nextValue = newItem;
                }
                else
                {
                    var lastVal = nextValue;
                    while (lastVal.next != null) lastVal = lastVal.next;
                    lastVal.next = newItem;
                }
                return;
            }

            // That is a protection from recursive Send() calls.
            iterating = true;

            var callbacksLocal = callbacks;

            iterateCallbacks:
            for (int i = 0; i < callbacksLocal.Count; i++)
            {
                callbacksLocal[i](t);
            }

            if (nextValue != null)
            {
                t = nextValue.item;
                callbacksLocal = nextValue.callbacks;
                nextValue = nextValue.next;
                goto iterateCallbacks;
            }

            iterating = false;
        }

        public IDisposable Subscribe(Action action)
        {
            if (callbacks == null) callbacks = new List<Action<T>>();
            Action<T> wrapper = _ => action();
            callbacks.Add(wrapper);
            return new Disconnect { reader = this, action = wrapper };
        }

        public int ConnectionsCount()
        {
            return callbacks != null ? callbacks.Count : 0;
        }

        public int getConnectionCount => callbacks == null ? 0 : callbacks.Count;
        public bool anybody => callbacks != null && callbacks.Count > 0;
    }

    /// Parametless variant of IEventReader
    public interface IEventReader
    {
        IDisposable Subscribe(Action action);
    }

    /// <summary>
    /// 
    /// </summary>
    public interface IEventWriter
    {
        void Send();
    }
    
    /// <summary>
    /// Event that can be sent and observed
    /// </summary>
    public interface IEventRW : IEventReader, IEventWriter {}
    
    /// Parametless variant of Event
    public class EventStream : IEventReader, IEventWriter, IConnectable
    {
        List<Action> callbacks;
        bool iterating;
        ValueListItem nextValue;

        class ValueListItem
        {
            public List<Action> callbacks;
            public ValueListItem next;
        }

        class Disconnect : IDisposable
        {
            public EventStream stream;
            public Action action;

            public void Dispose()
            {
                if (stream != null)
                {
                    stream.RemoveListener(action);
                    stream = null;
                    action = null;
                }
            }
        }

        void RemoveListener(Action action)
        {
            if (iterating)
            {
                callbacks = callbacks.ToList();
            }
            callbacks.Remove(action);
        }

        public void ClearCallbacks()
        {
            if (callbacks == null) return;
            if (iterating)
            {
                callbacks = callbacks.ToList();
            }
            callbacks.Clear();
        }

        public IDisposable Subscribe(Action action)
        {
            if (callbacks == null) callbacks = new List<Action>();
            else if (iterating) callbacks = callbacks.ToList();
            callbacks.Add(action);
            return new Disconnect {stream = this, action = action};
        }

        public void Send()
        {
            if (callbacks == null) return;

            if (iterating)
            {
                var newItem = new ValueListItem {callbacks = callbacks};
                if (nextValue == null)
                {
                    nextValue = newItem;
                }
                else
                {
                    ValueListItem lastVal = nextValue;
                    while (lastVal.next != null) lastVal = lastVal.next;
                    lastVal.next = newItem;
                }
                return;
            }

            iterating = true;

            var callbacksLocal = callbacks;

            iterateCallbacks:
            for (int i = 0; i < callbacksLocal.Count; i++)
            {
                callbacksLocal[i]();
            }

            if (nextValue != null)
            {
                callbacksLocal = nextValue.callbacks;
                nextValue = nextValue.next;
                goto iterateCallbacks;
            }

            iterating = false;
        }

        public int getConnectionCount => callbacks == null ? 0 : callbacks.Count;
    }

    class AbandonedReader : IEventReader
    {
        public IDisposable Subscribe(Action action)
        {
            return EmptyDisposable.value;
        }

        public static AbandonedReader value = new AbandonedReader();
    }

    class AbandonedReader<T> : IEventReader<T>
    {
        public IDisposable Subscribe(Action<T> action)
        {
            return EmptyDisposable.value;
        }

        public IDisposable Subscribe(Action action)
        {
            return EmptyDisposable.value;
        }

        public static AbandonedReader<T> value = new AbandonedReader<T>();
    }

    class AnonymousEventReader : IEventReader
    {
        readonly Func<Action, IDisposable> listen;

        public AnonymousEventReader(Func<Action, IDisposable> subscribe)
        {
            this.listen = subscribe;
        }

        public IDisposable Subscribe(Action observer)
        {
            return listen(observer);
        }
    }

    class AnonymousEventReader<T> : IEventReader<T>
    {
        readonly Func<Action<T>, IDisposable> listen;

        public AnonymousEventReader(Func<Action<T>, IDisposable> subscribe)
        {
            this.listen = subscribe;
        }

        public IDisposable Subscribe(Action<T> observer)
        {
            return listen(observer);
        }

        public IDisposable Subscribe(Action observer)
        {
            return listen(_ => observer());
        }
    }

    public static class StreamApi
    {
        public static IEventReader<T> Filter<T>(this IEventReader<T> eventReader, Func<T, bool> filter)
        {
            return new AnonymousEventReader<T>(reaction =>
            {
                return eventReader.Subscribe(val =>
                {
                    if (filter(val)) reaction(val);
                });
            });
        }
        
        public static IEventReader Filter(this IEventReader eventReader, Func<bool> filter)
        {
            return new AnonymousEventReader(reaction =>
            {
                return eventReader.Subscribe(() =>
                {
                    if (filter()) reaction();
                });
            });
        }

        public static IEventReader<T> Where<T>(this IEventReader<T> reader, Func<T, bool> predicate)
        {
            return reader.Filter(predicate);
        }

        public static IEventReader WhenTrue(this IEventReader<bool> reader)
        {
            return new AnonymousEventReader(reaction =>
            {
                return reader.Subscribe(v =>
                {
                    if (v) reaction();
                });
            });
        }

        // Transforms stream value with a function.
        public static IEventReader<T2> Map<T, T2>(this IEventReader<T> eventReader, Func<T, T2> map)
        {
            return new AnonymousEventReader<T2>(reaction => { return eventReader.Subscribe(val => reaction(map(val))); });
        }
        // Transforms stream value with a function.
        public static IEventReader<T2> Map<T2>(this IEventReader eventReader, Func<T2> map)
        {
            return new AnonymousEventReader<T2>(reaction => { return eventReader.Subscribe(() => reaction(map())); });
        }

        // Result stream is called only once, then the connection is disposed.
        public static IEventReader<T> Once<T>(this IEventReader<T> eventReader)
        {
            return new AnonymousEventReader<T>((Action<T> reaction) =>
            {
                var disp = new SingleDisposable();
                disp.Disposable = eventReader.Subscribe(val =>
                {
                    reaction(val);
                    disp.Dispose();
                });
                return disp;
            });
        }
        
        public static IDisposable ListenWhile<T>(this IEventReader<T> reader, ICell<bool> listenCondition, Action<T> act)
        {
            var disp = new DoubleDisposable();
            disp.first = listenCondition.Bind(val =>
            {
                if (val)
                {
                    if (disp.disposed) return;
                    if (disp.second != null)
                    {
                        throw new ZergRushException();
                    }
                    disp.second = reader.Subscribe(act);
                }
                else if (disp.second != null)
                {
                    disp.second.Dispose();
                    disp.second = null;
                }
            });
            return disp;
        }

        public static IEventReader Once(this IEventReader reader)
        {
            return new AnonymousEventReader((Action reaction) =>
            {
                var disp = new SingleDisposable();
                disp.Disposable = reader.Subscribe(() =>
                {
                    reaction();
                    disp.Dispose();
                });
                return disp;
            });
        }

        // Merge an array of streams info one stream.
        public static IEventReader<T> Merge<T>(params IEventReader<T>[] others)
        {
            if (others == null || others.Any(s => s == null)) throw new ArgumentException("Null streams in merge");
            return new AnonymousEventReader<T>((reaction) =>
            {
                var disp = new Connections(others.Length);

                foreach (var other in others)
                {
                    disp.Add(other.Subscribe(reaction));
                }

                return disp;
            });
        }
        public static IEventReader Merge(this IEnumerable<IEventReader> others)
        {
            return MergeSome(others.ToArray());
        }
        public static IEventReader MergeSome(params IEventReader[] others)
        {
            return new AnonymousEventReader((reaction) =>
            {
                var disp = new Connections(others.Length);

                for (var i = 0; i < others.Length; i++)
                {
                    var other = others[i];
                    disp.Add(other.Subscribe(reaction));
                }

                return disp;
            });
        }
        public static IEventReader<T> MergeWith<T>(this IEventReader<T> reader, params IEventReader<T>[] others)
        {
            if (reader == null || others == null || others.Any(s => s == null))
                throw new ArgumentException("Null streams in merge");
            return new AnonymousEventReader<T>((Action<T> reaction) =>
            {
                var disp = new Connections(others.Length + 1);
                disp.Add(reader.Subscribe(reaction));

                foreach (var other in others)
                {
                    disp.Add(other.Subscribe(reaction));
                }

                return disp;
            });
        }
        public static IEventReader MergeWith(this IEventReader reader, params IEventReader[] others)
        {
            if (reader == null || others == null || others.Any(s => s == null))
                throw new ArgumentException("Null streams in merge");
            return new AnonymousEventReader(reaction =>
            {
                var disp = new Connections(others.Length + 1);
                disp.Add(reader.Subscribe(reaction));
                foreach (var other in others)
                {
                    disp.Add(other.Subscribe(reaction));
                }

                return disp;
            });
        }
        static YieldAwaitable frame => Frame.one;
        public static async Task<T> SingleMessageAsync<T>(this IEventReader<T> reader)
        {
            T result = default(T);
            bool finished = false;
            var waiting = reader.Subscribe(res => { result = res; finished = true; });
            while (!finished)
                await frame;
            return result;
        }
        public static async Task SingleMessageAsync(this IEventReader reader)
        {
            bool finished = false;
            var waiting = reader.Subscribe(() => { finished = true; });
            while (!finished)
                await frame;
        }
    }
}