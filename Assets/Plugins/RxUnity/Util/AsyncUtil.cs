using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;

namespace RxUnity.Util {
    public class AsyncUtil {
        public static void DoWorkAsync(Action work) {
            ThreadPool.QueueUserWorkItem(stateinfo => work.Invoke());
        }

        public static IObservable<T> DoWorkAsync<T>(Func<T> work, IScheduler scheduler) {
            var asyncSubject = new AsyncSubject<T>();

            ThreadPool.QueueUserWorkItem(stateinfo => {
                try {
                    T result = work.Invoke();
                    asyncSubject.OnNext(result);
                    asyncSubject.OnCompleted();
                }
                catch (Exception e) {
                    asyncSubject.OnError(e);
                }
            });

            return asyncSubject.ObserveOn(scheduler)
                .SubscribeOn(scheduler);
        }
    }
}