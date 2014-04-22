using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using UnityEngine;

internal class NaiveUnityRxScheduler : UnityRxScheduler {
    private const long SecondInTicks = 10000000;

    private IList<RxTask> _futureTasks;

    private long _now;
    private IList<RxTask> _tasksToExecute;

    public override DateTimeOffset Now {
        get { return new DateTimeOffset(_now, TimeSpan.Zero); }
    }

    private void Awake() {
        _futureTasks = new List<RxTask>();
        _tasksToExecute = new List<RxTask>();
        DateTimeOffset beginningOfTime = DateTime.Now;
        _now = beginningOfTime.UtcTicks;
    }

    private void Update() {
        _now += (long) (Time.deltaTime*SecondInTicks);

        foreach (RxTask futureTask in _futureTasks) {
            if (futureTask.ScheduledTimeTicks <= _now) {
                _tasksToExecute.Add(futureTask);
            }
        }
        foreach (RxTask task in _tasksToExecute) {
            _futureTasks.Remove(task);
            task.Action();
        }
        _tasksToExecute.Clear();
    }

    public override IDisposable Schedule<TState>(TState state, DateTimeOffset dueTime,
        Func<IScheduler, TState, IDisposable> action) {
        return AddTask(dueTime.UtcTicks, () => action(this, state));
    }

    public override IDisposable Schedule<TState>(TState state, TimeSpan dueTime,
        Func<IScheduler, TState, IDisposable> action) {
        return AddTask(_now + dueTime.Ticks, () => action(this, state));
    }

    public override IDisposable Schedule<TState>(TState state, Func<IScheduler, TState, IDisposable> action) {
        return AddTask(_now + 1, () => action(this, state));
    }

    private IDisposable AddTask(long dueTime, Action action) {
        var task = new RxTask(dueTime, action);
        _futureTasks.Add(task);
        return Disposable.Create(() => _futureTasks.Remove(task));
    }
}

internal struct RxTask {
    private readonly Action _action;
    private readonly long _scheduledTimeTicks;

    public RxTask(long scheduledTimeTicks, Action action) {
        _action = action;
        _scheduledTimeTicks = scheduledTimeTicks;
    }

    public Action Action {
        get { return _action; }
    }

    public long ScheduledTimeTicks {
        get { return _scheduledTimeTicks; }
    }
}