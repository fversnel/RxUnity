using System;
using System.Collections;
using System.Reactive.Concurrency;
using UnityEngine;

/// <summary>
///     This class is not thread-safe as it uses Unity's StartCoroutine from the calling thread to schedule.
/// </summary>
internal class CoroutineScheduler : UnityRxScheduler {
    private DateTimeOffset _beginningOfTime;

    public override DateTimeOffset Now {
        get { return _beginningOfTime.AddSeconds(Time.time); }
    }

    private void Awake() {
        _beginningOfTime = DateTime.Now;
    }

    public override IDisposable Schedule<TState>(TState state, Func<IScheduler, TState, IDisposable> action) {
        DisposePair disposePair = DisposePair.Create();
        StartCoroutine(ScheduleImmediate(state, action, disposePair));
        return disposePair.Disposable;
    }

    public override IDisposable Schedule<TState>(TState state, TimeSpan dueTime,
        Func<IScheduler, TState, IDisposable> action) {
        DisposePair disposePair = DisposePair.Create();
        StartCoroutine(Schedule((float) dueTime.TotalSeconds, state, action, disposePair));
        return disposePair.Disposable;
    }

    public override IDisposable Schedule<TState>(TState state, DateTimeOffset dueTime,
        Func<IScheduler, TState, IDisposable> action) {
        TimeSpan scheduleTime = Now - dueTime;
        DisposePair disposePair = DisposePair.Create();
        StartCoroutine(Schedule((float) scheduleTime.TotalSeconds, state, action, disposePair));
        return disposePair.Disposable;
    }

    private IEnumerator ScheduleImmediate<TState>(TState state, Func<IScheduler, TState, IDisposable> action,
        DisposePair disposePair) {
        yield return new WaitForEndOfFrame();
        if (!disposePair.IsDisposed) {
            action(this, state);
        }
    }

    private IEnumerator Schedule<TState>(float seconds, TState state, Func<IScheduler, TState, IDisposable> action,
        DisposePair disposePair) {
        yield return new WaitForSeconds(seconds);
        if (!disposePair.IsDisposed) {
            action(this, state);
        }
    }
}

internal struct DisposePair {
    private readonly IDisposable _disposable;
    private DisposeState _disposeState;

    private DisposePair(IDisposable disposable, DisposeState disposeState) {
        _disposable = disposable;
        _disposeState = disposeState;
    }

    public IDisposable Disposable {
        get { return _disposable; }
    }

    public bool IsDisposed {
        get { return _disposeState.Value; }
    }

    public static DisposePair Create() {
        var disposeState = new DisposeState(false);
        return new DisposePair(System.Reactive.Disposables.Disposable.Create(() => disposeState.Value = true),
            disposeState);
    }

    private struct DisposeState {
        private bool _value;

        public DisposeState(bool value) {
            _value = value;
        }

        public bool Value {
            get { return _value; }
            set { _value = value; }
        }
    }
}