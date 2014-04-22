using System;
using System.Reactive.Concurrency;
using UnityEngine;

public abstract class UnityRxScheduler : MonoBehaviour, IScheduler {
    public abstract IDisposable Schedule<TState>(TState state, Func<IScheduler, TState, IDisposable> action);

    public abstract IDisposable Schedule<TState>(TState state, TimeSpan dueTime,
        Func<IScheduler, TState, IDisposable> action);

    public abstract IDisposable Schedule<TState>(TState state, DateTimeOffset dueTime,
        Func<IScheduler, TState, IDisposable> action);

    public abstract DateTimeOffset Now { get; }
}