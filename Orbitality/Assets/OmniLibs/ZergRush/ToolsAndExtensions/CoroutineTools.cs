#if UNITY_5_3_OR_NEWER

using System;
using System.Collections;
using Newtonsoft.Json;
using ZergRush.ReactiveCore;
using UnityEngine;

namespace ZergRush
{
    public class WaitResult<T>
    {
        public T value;
    }

    public class WaitForEvent : CustomYieldInstruction
    {
        IDisposable connection;
        bool ready;
        float timeout;
        public WaitForEvent(IEventReader reader, float timeout = -1)
        {
            this.timeout = timeout;
            connection = reader.Subscribe(() =>
            {
                connection.DisconnectSafe();
                ready = true;
                connection = null;
            });
            if (ready) connection.Dispose();
        }

        public override bool keepWaiting
        {
            get
            {
                if (timeout > 0)
                {
                    timeout -= Time.deltaTime;
                    if (timeout <= 0) return false;
                }
                return ready == false;
            }
        }
    }

    public class WaitForEvent<T> : CustomYieldInstruction
    {
        IDisposable connection;
        bool ready;
        public WaitForEvent(IEventReader<T> eventReader, WaitResult<T> result)
        {
            connection = eventReader.Subscribe(t =>
            {
                ready = true;
                result.value = t;
                connection.DisconnectSafe();
                connection = null;
            });
            if (ready) connection.Dispose();
        }
        public override bool keepWaiting { get { return ready == false; } }
    }
    
    public class RequestWaiter<TResult, TErr> : CustomYieldInstruction
    {
        bool ready;
        public Action<TResult> processSuccess;
        public Action<TErr> processFail;

        public bool success;
        public TResult Result;
        public TErr Error;
        
        public RequestWaiter()
        {
            processSuccess = r =>
            {
                success = true;
                Result = r;
                ready = true;
            };
            processFail = err => { 
                Error = err;
                ready = true;
            };
        }
        public override bool keepWaiting { get { return ready == false; } }
    }

    public class DoForSomeTime : IEnumerator
    {
        public float time;
        public Action action;

        public DoForSomeTime(float time, Action action)
        {
            this.time = time;
            this.action = action;
        }

        public bool MoveNext()
        {
            action();
            time -= Time.deltaTime;
            return time > 0;
        }

        public void Reset() {}

        public object Current => null;
    }
}

#endif