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
        /// <returns>An observable who's values are produced at most every update</returns>
        public static IObservable<T> EveryUpdate<T>(Action<IObserver<T>> publish) 
        {
            return FromCoroutine<T>((observer, isDisposed) => EveryUpdateCoroutine(publish, observer, isDisposed));
        }

        /// <summary>
        ///     <para>
        ///         Create an observable who's observer is called on every fixed update.
        ///     </para>
        /// </summary>
        /// <typeparam name="T">The observable's type</typeparam>
        /// <param name="publish">The method that is called each frame</param>
        /// <returns>An observable who's values are produced at most every fixed update</returns>
        public static IObservable<T> EveryFixedUpdate<T>(Action<IObserver<T>> publish)
        {
            return FromCoroutine<T>((observer, isDisposed) => EveryFixedUpdateCoroutine(publish, observer, isDisposed));
        }

        /// <summary>
        /// Creates an observable from a coroutine that consumes an observer and an isDisposed closure.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="coroutine">The coroutine that produces the values for the observable</param>
        /// <returns>an observable who's values are produced inside the given coroutine</returns>
        public static IObservable<T> FromCoroutine<T>(Func<IObserver<T>, Func<bool>, IEnumerator> coroutine)
        {
            return FromCoroutine(UnityThreadDispatcher.Instance.StartCoroutine, coroutine);
        }

        public static IObservable<T> FromCoroutine<T>(Action<IEnumerator> startCoroutine, 
            Func<IObserver<T>, Func<bool>, IEnumerator> coroutine)
        {
            return Observable.Create<T>(observer =>
            {
                var cancel = new BooleanDisposable();

                startCoroutine(coroutine(observer, () => cancel.IsDisposed));
                // Manage subscription inside Unity's lifecycle, complete observable when quitting etc.
                UnityApplicationLifecycle.Instance.OnQuit.Subscribe(_ => observer.OnCompleted());

                return cancel;
            });
        }

        public static IEnumerator EveryUpdateCoroutine<T>(Action<IObserver<T>> publish, 
            IObserver<T> observer, Func<bool> isDisposed) 
        {
            while (!isDisposed())
            {
                publish(observer);
                yield return null;
            }
        }

        public static IEnumerator EveryFixedUpdateCoroutine<T>(Action<IObserver<T>> publish, 
            IObserver<T> observer, Func<bool> isDisposed)
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

                    UnityThreadDispatcher.Instance.StartCoroutine(DelayFrameCore(() => x.Accept(observer), 
                        frameCount, () => cancel.IsDisposed));
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