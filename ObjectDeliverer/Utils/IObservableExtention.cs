using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace ObjectDeliverer.Utils
{
    public static class IObservableExtention
    {
        public static Task<T> WaitEvent<T>(this Subject<T> observable)
        {
            var tcs = new TaskCompletionSource<T>();
            IDisposable? subscription = observable.Subscribe(x =>
            {
                observable.Dispose();
                tcs.TrySetResult(x);
            });

            return tcs.Task;
        }
    }
}
