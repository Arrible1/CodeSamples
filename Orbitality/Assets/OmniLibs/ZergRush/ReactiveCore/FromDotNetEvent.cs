using System;

namespace ZergRush.ReactiveCore
{
    public class FromDotNetEvent
    {
        public static IEventReader Convert(Action<Action> subscribe, Action<Action> unsubscribe)
        {
            return new AnonymousEventReader((reaction) =>
            {
                subscribe(reaction);
                return new AnonymousDisposable(() =>
                {
                    unsubscribe(reaction);
                });
            });
        }
    }

    public class FromDotNetEvent<T>
    {
        public static IEventReader<T> Convert(Action<Action<T>> subscribe, Action<Action<T>> unsubscribe)
        {
            return new AnonymousEventReader<T>((reaction) =>
            {
                subscribe(reaction);
                return new AnonymousDisposable(() =>
                {
                    unsubscribe(reaction);
                });
            });
        }
    }
}