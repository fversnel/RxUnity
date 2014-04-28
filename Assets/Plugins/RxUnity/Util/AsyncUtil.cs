using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace RxUnity.Util {

    public class AsyncUtil {

        public static IObservable<T> RunAsync<T>(Func<T> work) {
            var resultHolder = new AsyncSubject<T>();

            Task.Factory.StartNew(() =>
            {
                try
                {
                    T result = work();
                    resultHolder.OnNext(result);
                    resultHolder.OnCompleted();
                }
                catch (Exception e)
                {
                    resultHolder.OnError(e);
                }
            });

            return resultHolder;
        }
    }
}