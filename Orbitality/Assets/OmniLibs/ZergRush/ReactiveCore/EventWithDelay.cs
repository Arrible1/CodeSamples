using System;

namespace ZergRush.ReactiveCore
{
    [CLIENT]
    public class EventWithDelay<T> : IEventRW<T>
    {
        readonly float delay;
        EventStream<T> receiving = new EventStream<T>();
        readonly bool realtime;
        public EventWithDelay(float delay, bool realtime) { this.delay = delay; this.realtime = realtime; }
        public void Send(T val)
        {
            if (realtime)
                ReactiveTimeInteractions.ExecuteAfterRealtimeDelay(delay, () => receiving.Send(val));
            else
                ReactiveTimeInteractions.ExecuteAfterDelay(delay, () => receiving.Send(val));
        }
        public IDisposable Subscribe(Action<T> action) => receiving.Subscribe(action);
        public IDisposable Subscribe(Action action) => receiving.Subscribe(action);
    }
}