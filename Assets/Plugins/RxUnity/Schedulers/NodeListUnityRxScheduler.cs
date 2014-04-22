using System;
using System.Reactive.Concurrency;
using RxUnity.Schedulers;
using UnityEngine;

public class NodeListUnityRxScheduler : UnityRxScheduler {
    private const long SecondInTicks = 10000000;

    private NodeList<long, Action> _futureTasks;

    private long _now;

    public override DateTimeOffset Now {
        get { return new DateTimeOffset(_now, TimeSpan.Zero); }
    }

    private void Awake() {
        _futureTasks = new NodeList<long, Action>();

        DateTimeOffset beginningOfTime = DateTime.Now;
        _now = beginningOfTime.UtcTicks;
    }

    private void Update() {
        _now += (long) (Time.deltaTime*SecondInTicks);

        NodeList<long, Action>.Node node = _futureTasks.FirstNode();
        while (node != null && node.Key <= _now) {
            node.FlagAsDeleted();
            PerformAction(node.Value);
            node = _futureTasks.FirstNode();
        }
    }

    private void PerformAction(Action action) {
        action();
    }

    public override IDisposable Schedule<TState>(TState state, DateTimeOffset dueTime,
        Func<IScheduler, TState, IDisposable> action) {
        return _futureTasks.Add(dueTime.UtcTicks, () => action(this, state));
    }

    public override IDisposable Schedule<TState>(TState state, TimeSpan dueTime,
        Func<IScheduler, TState, IDisposable> action) {
        return _futureTasks.Add(_now + dueTime.Ticks, () => action(this, state));
    }

    public override IDisposable Schedule<TState>(TState state, Func<IScheduler, TState, IDisposable> action) {
        return _futureTasks.Add(_now + 1, () => action(this, state));
    }
}