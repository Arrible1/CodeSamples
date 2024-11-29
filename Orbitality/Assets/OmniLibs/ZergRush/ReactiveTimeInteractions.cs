#if UNITY_5_3_OR_NEWER

using System;
using System.Collections;
using DG.Tweening;
using DG.Tweening.Core.Easing;
using UnityEngine;
using Random = UnityEngine.Random;
using ZergRush.ReactiveCore;

namespace ZergRush
{
    public static partial class ReactiveTimeInteractions
    {
        class AnonymousUpdatable : IUpdatable
        {
            Action update;
            public AnonymousUpdatable(Action update)
            {
                this.update = update;
            }
            public void Update(float dt)
            {
                update?.Invoke();
            }
        }

        class TriggerCell : Cell<float>, IUpdatable
        {
            public float decay;
            public void Reset()
            {
                value = decay;
            }
            public void Update(float dt)
            {
                value = Mathf.Max(value - dt, 0);
            }
        }
        
        // Satanic programming.
        public class CellOfSin : Cell<float>, IUpdatable
        {
            public float scale;
            public float time;
            public float speed = 1;

            public float offset = 0;
            
            public void Reset()
            {
                time = 0;
            }
            
            public void Reset(float val)
            {
                time = val;
            }
            
            public void Update(float dt)
            {
                time += dt * speed;
                value = offset + Mathf.Abs(Mathf.Sin(time)) * scale;
            }
        }
        
        class SpikeCell : Cell<float>, IUpdatable
        {
            public float attackPoint;
            public float platoPoint;
            public float decayPoint;

            float curr = 100000;

            public void Reset()
            {
                curr = 0;
            }
            
            public void Update(float dt)
            {
                curr += dt;
                
                if (curr < attackPoint) 
                    value = curr / attackPoint;
                else if (curr < platoPoint)
                    value = 1;
                else if (curr < decayPoint)
                    value = (decayPoint - curr) / (decayPoint - platoPoint);
                else
                    value = 0;
            }
        }
        
        public static IEventReader EachFrameEvent()
        {
            return UnityExecutor.instance.eachFrame;
        }

        public static void ExecuteEachFrame(Action action, Action<IDisposable> connectionSink)
        {
            connectionSink(UnityExecutor.instance.eachFrame.Subscribe(action));
        }
        
        class FrameDelayedAction : IUpdatable
        {
            private int remainingTime;
            Action action;
            IDisposable connection;
            public FrameDelayedAction(int delay, Action action)
            {
                remainingTime = delay;
                this.action = action;
                connection = UnityExecutor.instance.AddUpdatable(this);
            }
            public void Update(float dt)
            {
                remainingTime--;
                if (remainingTime == 0)
                {
                    connection.Dispose();
                    action();
                }
            }
        }

        class WaitingDelayedAction : IUpdatable
        {
            private float remainingTime;
            Action action;
            IDisposable connection;
            public WaitingDelayedAction(float delay, Action action, bool realtime = false)
            {
                remainingTime = delay;
                this.action = action;
                if (realtime)
                    connection = UnityExecutor.instance.AddRealtimeUpdatable(this);
                else
                    connection = UnityExecutor.instance.AddUpdatable(this);
            }
            public void Update(float dt)
            {
                remainingTime -= dt;
                if (remainingTime < 0)
                {
                    connection.Dispose();
                    action();
                }
            }
        }

        public static void ExecuteAfterCondition(Func<bool> condition, Action action)
        {
            if (condition())
                action();
            else
            {
                IDisposable updating = null;
                updating = UnityExecutor.instance.eachFrame.Subscribe(() =>
                {
                    if (!condition())
                        return;
                    updating.Dispose();
                    action();
                });
            }
        }
        public static void ExecuteAfterDelay(float delay, Action action)
        {
            new WaitingDelayedAction(delay, action);
        }
        public static void ExecuteAfterRealtimeDelay(float delay, Action action)
        {
            new WaitingDelayedAction(delay, action, true);
        }        
        public static void ExecuteNextUpdate(Action action)
        {
            new FrameDelayedAction(1, action);
        }
        public static void ExecuteAfterFrames(int frames, Action action)
        {
            new FrameDelayedAction(frames, action);
        }

        class Resampler : ICell<float>, IUpdatable
        {
            public ICell<float> source;
            EventStream<float> update;
            public void Update(float dt)
            {
                update.Send(source.value);
            }

            public IDisposable ListenUpdates(Action<float> reaction)
            {
                return update.Subscribe(reaction);
            }

            public float value => source.value;
        }
        // 
        public static ICell<float> Resample(this ICell<float> signal, Action<IDisposable> connectionSink)
        {
            var resampler = new Resampler {source = signal};
            connectionSink(UnityExecutor.instance.AddUpdatable(resampler));
            return resampler;
        }
        
        class EaseCell : Cell<float>, IUpdatable
        {
            public float startValue;
            public float targetValue;
            public Ease func;
            public float currTime;
            public float period;
            public void Set(float newVal)
            {
                startValue = value;
                targetValue = newVal;
                currTime = 0;
            }
            public void Update(float dt)
            {
                if (currTime >= period) return;

                currTime += dt;
                if (currTime > period)
                {
                    currTime = period;
                }
                var val = EaseManager.Evaluate(func, null, currTime, period, 1, 1);
                value = startValue + (targetValue - startValue) * val;
            }
        }
        
        // ease changes of your value
        public static ICell<float> EaseChanges(this ICell<float> e, Ease ease, float easeTime, Action<IDisposable> connectionSink)
        {
            var cell = new EaseCell(){startValue = e.value, currTime = easeTime, period = easeTime, func = ease};
            cell.value = e.value;
            connectionSink(UnityExecutor.instance.AddUpdatable(cell));
            connectionSink(e.ListenUpdates(cell.Set));
            return cell;
        }

        public class LaggedCell<T> : Cell<T>, IUpdatable
        {
            public LaggedCell(ICell<T> source, float lag = 1, Predicate<T> isValueLagged = null, Action<IDisposable> connectionSink = null)
            {
                value = source.value;
                connectionSink(UnityExecutor.instance.AddUpdatable(this));
                connectionSink(source.ListenUpdates(val =>
                {
                    if (isValueLagged == null || isValueLagged(val))
                    {
                        laggedValue = val;
                        lagRemaining = lag;
                    }
                    else
                    {
                        value = val;
                        lagRemaining = -1; // Kill currently lagged value, it will never be shown.
                    }
                }));
            }
            T laggedValue;
            float lagRemaining = -1;
            public void Update(float dt)
            {
                if (lagRemaining <= 0) return;
                lagRemaining -= dt;
                if (lagRemaining <= 0)
                    value = laggedValue;
            }
        }

        public static ICell<T> Lag<T>(this ICell<T> e, float lag = 1, Predicate<T> isValueLagged = null, Action<IDisposable> connectionSink = null)
            => new LaggedCell<T>(e, lag, isValueLagged, connectionSink);

        // On each event it makes |\____|\_______|\_____....
        public static ICell<float> SignalTrigger(this IEventReader e, float decayTime, Action<IDisposable> connectionSink)
        {
            TriggerCell cell = new TriggerCell{decay = decayTime};
            connectionSink(UnityExecutor.instance.AddUpdatable(cell));
            connectionSink(e.Subscribe(cell.Reset));
            return cell;
        }

        // On each event it makes /--\____/--\________....
        public static ICell<float> SignalSpike(this IEventReader e, float attack, float plato, float decay, Action<IDisposable> connectionSink)
        {
            SpikeCell cell = new SpikeCell
            {
                attackPoint = attack,
                platoPoint = attack + plato,
                decayPoint = attack + plato + decay
            };
            connectionSink(UnityExecutor.instance.AddUpdatable(cell));
            connectionSink(e.Subscribe(cell.Reset));
            return cell;
        }
        
        public static CellOfSin SignalSin(this IEventReader e, float scale, float speed, float resetVal, Action<IDisposable> connectionSink)
        {
            CellOfSin cell = new CellOfSin { scale = scale , speed = speed};
            connectionSink(UnityExecutor.instance.AddUpdatable(cell));
            connectionSink(e.Subscribe(() => cell.Reset(resetVal)));
            return cell;
        }
        
        // Perlin from -1 to +1
        public static ICell<float> SignalShake(float speed)
        {
            var seed = Random.value * 100000;
            return UnityExecutor.instance.time.Map(val => Mathf.PerlinNoise(val * speed, seed));
        }
        
        // Perlin from -1 to +1
        public static ICell<Vector2> SignalShakeV2(float speed)
        {
            var seed = Random.value * 100000;
            var seed2 = Random.value * 100000;
            return UnityExecutor.instance.time.Map(val => 
                new Vector2(
                    (Mathf.PerlinNoise(val * speed, seed) - 0.5f) * 2, 
                    (Mathf.PerlinNoise(val * speed, seed2) - 0.5f) * 2)
            );
        }
        
        public static IEventReader<float> TimeAccumulator(this ICell<float> val, float interval, Action<IDisposable> connectionSink)
        {
            var accum = 0f;
            connectionSink(val.Bind(v => accum += v));
            return UnityExecutor.instance.TickStream(interval).Map(() =>
            {
                var currAcc = accum;
                accum = 0;
                return currAcc;
            });
        }
        
        public static IEventReader GlobalInterval(float timeInterval)
        {
            return UnityExecutor.instance.TickStream(timeInterval);
        }

        public class TickUpdatable : EventStream, IUpdatable
        {
            float interval;
            float cur;

            public TickUpdatable(float interval)
            {
                this.interval = interval;
            }

            public void Update(float dt)
            {
                cur += dt;
                if (cur > interval)
                {
                    cur = 0;
                    Send();
                }
            }
        }

        public static IEventReader Interval(float timeInterval, Action<IDisposable> connectionSink)
        {
            var e = new TickUpdatable(timeInterval);
            connectionSink(UnityExecutor.instance.AddUpdatable(e));
            return e;
        }

        public static IEventReader Interval30FPS()
        {
            return UnityExecutor.instance.TickStream(1 / 30f);
        }
        
        public static IEventReader Interval10FPS()
        {
            return UnityExecutor.instance.TickStream(1 / 10f);
        }

        class CycleBuffer<T>
        {
            int currentIndex = -1;
            int total;
            int max;
            T[] values;
            
            public CycleBuffer(int sampleCount)
            {
                max = sampleCount;
                values = new T[sampleCount];
            }

            public CycleBuffer()
            {
            }

            // index == 0 is the last one, 1 is the previous and so on
            public void ForEach(Action<int, T> action)
            {
                //Debug.Log(values.PrintCollection());
                if (currentIndex < 0) return;
                int limit = Mathf.Min(total, max);
                for (int i = 0; i < limit; i++)
                {
                    action(i, values[(currentIndex - i + max) % max]);
                }
            }

            public void Push(T value)
            {
                currentIndex = (currentIndex + 1) % max;
                if (total != max) total = currentIndex + 1;
                values[currentIndex] = value;
            }
        }

        public static ICell<float> GaussFilter(this ICell<float> value, int samples,
            float sampleInterval, Action<IDisposable> connectionSink, bool prefil = false)
        {
            Cell<float> newValue = new Cell<float>();
            CycleBuffer<float> valueBuffer = new CycleBuffer<float>(samples);
            if (prefil)
            {
                for (int i = 0; i < samples; i++)
                {
                    valueBuffer.Push(value.value);
                }
            }

            connectionSink(UnityExecutor.instance.TickStream(sampleInterval).Subscribe(() => {
                valueBuffer.Push(value.value);
                float sigma = samples * samples * 2;
                float weightAccum = 0;
                float valAccum = 0;
                valueBuffer.ForEach((i, v) =>
                {
                    var gauss = Mathf.Exp(-i * i / sigma);
                    weightAccum += gauss;
                    valAccum += gauss * v;
                });
                
//                Debug.Log("value accum: " + valAccum);
//                Debug.Log("weight accum: " + weightAccum);
                
                valAccum /= weightAccum;
                newValue.value = valAccum;
            }));
            return newValue;
        }
        
        public static ICell<Vector3> GaussFilter(this ICell<Vector3> value, int samples,
            float sampleInterval, Action<IDisposable> connectionSink)
        {
            Cell<Vector3> newValue = new Cell<Vector3>();
            CycleBuffer<Vector3> valueBuffer = new CycleBuffer<Vector3>(samples);
            connectionSink(UnityExecutor.instance.TickStream(sampleInterval).Subscribe(() => {
                valueBuffer.Push(value.value);
                float sigma = samples * samples * 2;
                float weightAccum = 0;
                Vector3 valAccum = Vector3.zero;
                valueBuffer.ForEach((i, v) =>
                {
                    var gauss = Mathf.Exp(-i * i / sigma);
                    weightAccum += gauss;
                    valAccum += gauss * v;
                });
                
//                Debug.Log("value accum: " + valAccum);
//                Debug.Log("weight accum: " + weightAccum);
                
                valAccum /= weightAccum;
                newValue.value = valAccum;
            }));
            return newValue;
        }
        
        //TODO- Make valid in the first tick
        public static ICell<float> Derivative(this ICell<float> value, float sampleInterval, Action<IDisposable> connectionSink)
        {
            Cell<float> derivative = new Cell<float>();
            float lastValue = value.value;
            connectionSink(UnityExecutor.instance.TickStream(sampleInterval).Subscribe(() =>
            {
                var newVal = value.value;
                derivative.value = (newVal - lastValue) / sampleInterval;
                lastValue = newVal;
            }));
            return derivative;
        }
        
        public static ICell<float> MovementSpeed(this ICell<Vector3> value, float sampleInterval, Action<IDisposable> connectionSink)
        {
            Cell<float> derivative = new Cell<float>();
            var lastValue = value.value;
            connectionSink(UnityExecutor.instance.TickStream(sampleInterval).Subscribe(() =>
            {
                var newVal = value.value;
                derivative.value = Vector3.Distance(newVal, lastValue) / sampleInterval;
                lastValue = newVal;
            }));
            return derivative;
        }

        public static IDisposable StartCoroutine(IEnumerator coro)
        {
            var coroHandle = UnityExecutor.instance.StartCoroutine(coro);
            return new AnonymousDisposable(() => UnityExecutor.instance.StopCoroutine(coroHandle));
        }
    }
}

#endif