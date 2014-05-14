using System;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace RxUnity.Util {

    public class AsyncUtil {

        /// <summary>
        /// Runs the work asynchronously then publishes and stores it in the returned Observable.
        /// </summary>
        /// <typeparam name="T">the result type of the work</typeparam>
        /// <param name="work">the actual work to be done</param>
        /// <returns>an observable that will hold the result of the work when it is done</returns>
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