using System;
using System.Collections.Generic;
using ZergRush.Alive;
using ZergRush.Alive;
using ZeroLag;

namespace ZergRush.ReactiveCore
{
    /*
     *     Special structs for replicative-model-multiplayer event sending
     */
    
    public static class GlobEventSettings
    {
        public const int usefullnessStepCount = 20;
    }
    
    [GenSimpleData]
    public partial struct EventBuffer<T> where T : IHashable
    {
        public struct EventRecord
        {
            public int step;
            public T e;
        }

        [GenIgnore] public List<EventRecord> events;
        [GenIgnore] public DataRoot root;
        [GenIgnore] public IDataRootWithStep rootS => (IDataRootWithStep) root;

        public void Send(T t)
        {
            if (events == null) events = new List<EventRecord>();
            events.Add(new EventRecord{e = t, step = rootS.step});
        }
    }
    
    public class EventMemory<TEvent> where TEvent : IHashable
    {
        public struct Mem
        {
            public int step;
            public ulong hash;
        }
        public List<Mem> eventsRememberred = new List<Mem>();
        public List<TEvent> currOut = new List<TEvent>();

        public List<TEvent> ReadEvents(EventBuffer<TEvent> buffer)
        {
            return ReadEvents(buffer.events, buffer.rootS.step);
        }
        
        public List<TEvent> ReadEvents(LCompositeEvent<TEvent> buffer)
        {
            return ReadEvents(buffer.uiEvent.events, buffer.uiEvent.rootS.step);
        }
        
        public List<TEvent> ReadEvents(List<EventBuffer<TEvent>.EventRecord> newEvents, int currStep)
        {
            currOut.Clear();
            if (newEvents == null) return currOut;
            
            // Clear old no longer actual events in memory
            for (var i = eventsRememberred.Count - 1; i >= 0; i--)
            {
                var step = eventsRememberred[i].step;
                if (currStep > step + GlobEventSettings.usefullnessStepCount)
                {
                    eventsRememberred.RemoveAt(i);
                }
            }
            
            for (var i = 0; i < newEvents.Count; i++)
            {
                var e = newEvents[i];
                if (currStep - e.step > GlobEventSettings.usefullnessStepCount)
                    continue;
                var hash = e.e.CalculateHash();
                bool found = false;
                for (var j = 0; j < eventsRememberred.Count; j++)
                {
                    var eventMemoryEvent = eventsRememberred[j].hash;
                    if (hash == eventMemoryEvent)
                    {
                        found = true;
                        break;
                    }
                }

                if (found == false)
                {
                    eventsRememberred.Add(new Mem{hash = hash, step = e.step});
                    currOut.Add(e.e);
                }
            }
            newEvents.Clear();

            return currOut;
        }
    }

    [GenSimpleData]
    public partial struct LCompositeEvent<T> where T : IHashable
    {
        [GenIgnore] public List<Action<T>> callbacks;
        public EventBuffer<T> uiEvent;

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
            // Battle event
            if (callbacks != null)
            {
                for (int i = 0; i < callbacks.Count; i++)
                {
                    callbacks[i](t);
                }
            }

            // UI event
            uiEvent.Send(t);
        }
        
        public DataRoot root
        {
            set { uiEvent.root = value; }
        }
    }
}