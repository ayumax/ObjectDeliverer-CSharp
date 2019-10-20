using System;
using System.Threading;
using System.Threading.Tasks;

namespace ObjectDeliverer.Utils
{
    public class MutexLocker : IDisposable
    {
        private Mutex mutex;

        public MutexLocker(string mutexName)
        {
            mutex = new Mutex(false, mutexName);
        }

        public void Lock(Action action)
        {
            mutex.WaitOne();

            try
            {
                action.Invoke();
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }

        public async ValueTask LockAsync(Func<ValueTask> action)
        {
            mutex.WaitOne();

            try
            {
                await action.Invoke();
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // 重複する呼び出しを検出するには

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    mutex?.Dispose();
                }

                disposedValue = true;
            }
        }

     
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
