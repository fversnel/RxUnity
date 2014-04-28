using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using RxUnity.Schedulers;
using RxUnity.Utils;
using UnityEngine;

namespace RxUnity.Core
{
    public static class UnityObservable
    {
        /// <summary>
        ///     <para>
        ///         Create an observable who's observer is called on every update.
        ///     </para>
        /// </summary>
        /// <typeparam name="T">The observable's type</typeparam>
        /// <param name="publish">The method that is called each frame</param>
        /// <returns>An observable who's events are produced at most every update</returns>
        public static IObservable<T> EveryUpdate<T>(Action<IObserver<T>> publish) 
        {
            return EveryUpdate(UnityThreadDispatcher.Instance.StartCoroutine, EveryUpdateCoroutine, publish);
        }

        /// <summary>
        ///     <para>
        ///         Create an observable who's observer is called on every fixed update.
        ///     </para>
        /// </summary>
        /// <typeparam name="T">The observable's type</typeparam>
        /// <param name="publish">The method that is called each frame</param>
        /// <returns>An observable who's events are produced at most every fixed update</returns>
        public static IObservable<T> EveryFixedUpdate<T>(Action<IObserver<T>> publish)
        {
            return EveryUpdate(UnityThreadDispatcher.Instance.StartCoroutine, EveryFixedUpdateCoroutine, publish);
        }

        public static IObservable<T> EveryUpdate<T>(Action<IEnumerator> startUpdateRoutine, 
            Func<IObserver<T>, Action<IObserver<T>>, Func<bool>, IEnumerator> updateRoutine,
            Action<IObserver<T>> publish)
        {
            return Observable.Create<T>(observer =>
            {
                var cancel = new BooleanDisposable();

                startUpdateRoutine(updateRoutine(observer, publish, () => cancel.IsDisposed));
                // Manage subscription inside Unity's lifecycle, complete observable when quitting etc.
                UnityApplicationLifecycle.Instance.OnQuit.Subscribe(x =>
                {
                    observer.OnCompleted();
                    // Dispose this thing just to be sure, shouldn't be necessary since 
                    // the UnityThreadDispatcher will be killed anyway
                    cancel.Dispose();
                });

                return cancel;
            });
        }

        public static IEnumerator EveryUpdateCoroutine<T>(IObserver<T> observer, Action<IObserver<T>> publish, Func<bool> isDisposed) 
        {
            while (!isDisposed())
            {
                publish(observer);
                yield return null;
            }
        }

        public static IEnumerator EveryFixedUpdateCoroutine<T>(IObserver<T> observer, Action<IObserver<T>> publish, Func<bool> isDisposed)
        {
            while (!isDisposed())
            {
                yield return new WaitForFixedUpdate();
                publish(observer);
            }
        }

        // TODO This still needs work, we need to be aware of Unity's lifecycle,
        // also maybe this can be simpler implemented, using the EveryUpdate method
        public static IObservable<T> DelayFrame<T>(this IObservable<T> source, int frameCount)
        {
            if (frameCount < 0) throw new ArgumentOutOfRangeException("frameCount");

            return Observable.Create<T>(observer =>
            {
                var cancel = new BooleanDisposable();

                source.Materialize().Subscribe(x =>
                {
                    if(x.Kind == NotificationKind.OnError)
                    {
                        observer.OnError(x.Exception);
                        cancel.Dispose();
                        return;
                    }

                    UnityThreadDispatcher.Instance.StartCoroutine(DelayFrameCore(() => x.Accept(observer), frameCount, () => cancel.IsDisposed));
                });

                return cancel;
            });
        }

        static IEnumerator DelayFrameCore(Action onNext, int frameCount, Func<bool> isDisposed)
        {
            while (!isDisposed() && frameCount-- != 0)
            {
                yield return null;
            }
            if (!isDisposed())
            {
                onNext();
            }
        }

        public static IObservable<T> ObserveOnUnityThread<T>(this IObservable<T> source)
        {
            return source
                .ObserveOn(UnityThreadScheduler.MainThread)
                .SubscribeOn(UnityThreadScheduler.MainThread);
        }
    }
}